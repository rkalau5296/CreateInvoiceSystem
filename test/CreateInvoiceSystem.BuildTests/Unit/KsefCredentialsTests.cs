using CreateInvoiceSystem.API.Repositories.KsefCredentialsRepository;
using CreateInvoiceSystem.API.Repositories.KsefOutboxRepository;
using CreateInvoiceSystem.API.DI;
using CreateInvoiceSystem.Invoices.Persistence.Shared.Entities;
using CreateInvoiceSystem.Modules.Ksef.Application.Handlers;
using CreateInvoiceSystem.Modules.Ksef.Application.Services;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetIncomingKsefInvoices;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceDetails;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceXml;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoicePdf;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.SendInvoiceToKsef;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.KsefSettings;
using CreateInvoiceSystem.Modules.Ksef.Dto;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using CreateInvoiceSystem.Modules.Ksef.Controllers;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Shared.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MediatR;
using System.Security.Claims;
using System.Text.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class KsefCredentialsTests
{
    [Fact]
    public void Protected_tokens_remain_readable_after_restart_with_persisted_certificate_keys()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ksef-dp-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=KsefTest", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment, true));
            using var cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
            var certPath = Path.Combine(root, "certificate.pfx");
            File.WriteAllBytes(certPath, cert.Export(X509ContentType.Pfx, "test-password"));
            var keysPath = Path.Combine(root, "keys");
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["KsefDataProtection:KeyDirectory"] = keysPath,
                ["KsefDataProtection:CertificatePath"] = certPath,
                ["KsefDataProtection:CertificatePassword"] = "test-password"
            }).Build();
            var environment = new Mock<IHostEnvironment>();
            environment.SetupGet(x => x.EnvironmentName).Returns("Production");
            using var first = CreateDataProtectionProvider(config, environment.Object);
            var encrypted = first.GetRequiredService<IDataProtectionProvider>().CreateProtector("test").Protect("secret");
            using var second = CreateDataProtectionProvider(config, environment.Object);
            Assert.Equal("secret", second.GetRequiredService<IDataProtectionProvider>().CreateProtector("test").Unprotect(encrypted));
            Assert.DoesNotContain("secret", File.ReadAllText(Assert.Single(Directory.GetFiles(keysPath))));
            Assert.Contains("encryptedSecret", File.ReadAllText(Assert.Single(Directory.GetFiles(keysPath))));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Production_without_key_configuration_fails_closed()
    {
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(x => x.EnvironmentName).Returns("Production");
        var config = new ConfigurationBuilder().Build();
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddKsefDataProtection(config, environment.Object));
    }

    private static ServiceProvider CreateDataProtectionProvider(IConfiguration config, IHostEnvironment environment)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKsefDataProtection(config, environment);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Credentials_are_encrypted_isolated_bound_to_nip_and_deleted_with_user()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(ct);
        var options = new DbContextOptionsBuilder<CreateInvoiceSystemDbContext>().UseSqlite(connection).Options;
        await using var db = new CreateInvoiceSystemDbContext(options);
        await db.Database.EnsureCreatedAsync(ct);
        var first = new UserEntity { UserName = "first", Nip = "1234567890", Address = new AddressEntity() };
        var second = new UserEntity { UserName = "second", Nip = "9876543210", Address = new AddressEntity() };
        db.Users.AddRange(first, second);
        await db.SaveChangesAsync(ct);

        var store = new KsefCredentialsRepository(db, new EphemeralDataProtectionProvider());
        await store.SaveAsync(first.Id, "first-secret", ct);
        await store.SaveAsync(second.Id, "second-secret", ct);
        await db.SaveChangesAsync(ct);
        var firstStored = await db.KsefCredentials.AsNoTracking().SingleAsync(x => x.UserId == first.Id, ct);
        var secondStored = await db.KsefCredentials.SingleAsync(x => x.UserId == second.Id, ct);
        Assert.DoesNotContain("first-secret", firstStored.ProtectedToken);
        Assert.DoesNotContain("second-secret", secondStored.ProtectedToken);
        Assert.Equal("first-secret", (await store.GetRequiredAsync(first.Id, ct)).Token);
        Assert.Equal("second-secret", (await store.GetRequiredAsync(second.Id, ct)).Token);
        await Assert.ThrowsAsync<ArgumentException>(() => store.SaveAsync(first.Id, " ", ct));
        Assert.Equal("first-secret", (await store.GetRequiredAsync(first.Id, ct)).Token);

        var invoice = new InvoiceEntity { UserId = first.Id, Title = "Invoice 1" };
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(ct);
        var outbox = new KsefOutboxRepository(db, db);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => outbox.EnqueueAsync(second.Id, invoice.InvoiceId, ct));
        Assert.False(await db.KsefOutgoingInvoiceStatuses.AnyAsync(ct));
        db.Invoices.Remove(invoice);
        await db.SaveChangesAsync(ct);

        secondStored.ProtectedToken = firstStored.ProtectedToken;
        await db.SaveChangesAsync(ct);
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.GetRequiredAsync(second.Id, ct));

        first.Nip = "1111111111";
        await db.SaveChangesAsync(ct);
        Assert.False(await store.IsConfiguredAsync(first.Id, ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.GetRequiredAsync(first.Id, ct));
        await store.SaveAsync(first.Id, "replacement-secret", ct);
        await db.SaveChangesAsync(ct);
        Assert.Equal("replacement-secret", (await store.GetRequiredAsync(first.Id, ct)).Token);

        await store.DeleteAsync(first.Id, ct);
        await db.SaveChangesAsync(ct);
        Assert.False(await store.IsConfiguredAsync(first.Id, ct));
        await store.SaveAsync(first.Id, "new-secret", ct);
        await db.SaveChangesAsync(ct);
        db.Users.Remove(first);
        await db.SaveChangesAsync(ct);
        Assert.False(await db.KsefCredentials.AsNoTracking().AnyAsync(x => x.UserId == first.Id, ct));
    }

    [Fact]
    public async Task Settings_and_invoice_download_require_user_claim_and_use_it()
    {
        var ct = TestContext.Current.CancellationToken;
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var controller = new KsefController(mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        Assert.IsType<UnauthorizedResult>(await controller.DownloadInvoiceXmlAsync("number", ct));
        Assert.IsType<UnauthorizedResult>(await controller.GetInvoicePdfAsync("number", ct));
        Assert.IsType<UnauthorizedResult>(await controller.SaveSettingsAsync(new SaveKsefSettingsDto { Token = "secret" }, ct));

        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "42")], "test"));
        mediator.Setup(x => x.Send(It.Is<GetKsefInvoiceXmlRequest>(r => r.UserId == 42 && r.KsefNumber == "number"), ct))
            .ReturnsAsync("<Faktura/>");
        mediator.Setup(x => x.Send(It.Is<GetKsefInvoicePdfRequest>(r => r.UserId == 42 && r.KsefNumber == "number"), ct))
            .ReturnsAsync([1, 2, 3]);
        mediator.Setup(x => x.Send(It.Is<GetKsefSettingsRequest>(r => r.UserId == 42), ct))
            .ReturnsAsync(true);
        mediator.Setup(x => x.Send(It.Is<SaveKsefSettingsRequest>(
                r => r.UserId == 42 && r.Token == "secret"), ct))
            .Returns(Task.CompletedTask);
        Assert.IsType<FileContentResult>(await controller.DownloadInvoiceXmlAsync("number", ct));
        var pdf = Assert.IsType<FileContentResult>(await controller.GetInvoicePdfAsync("number", ct));
        Assert.Equal("application/pdf", pdf.ContentType);
        Assert.Equal("KSeF_number.pdf", pdf.FileDownloadName);
        Assert.IsType<NoContentResult>(await controller.SaveSettingsAsync(new SaveKsefSettingsDto { Token = "secret" }, ct));
        var status = Assert.IsType<OkObjectResult>(await controller.GetSettingsAsync(ct));
        Assert.Equal("{\"IsConfigured\":true}", JsonSerializer.Serialize(status.Value));
        Assert.Equal("no-store", controller.Response.Headers.CacheControl.ToString());
    }

    [Fact]
    public async Task Invoice_queries_use_the_request_user_for_ksef_authentication()
    {
        var api = new Mock<IKsefApiClient>(MockBehavior.Strict);
        var ct = TestContext.Current.CancellationToken;
        api.Setup(x => x.GetIncomingInvoicesAsync(42, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), 0, 20, ct))
            .ReturnsAsync(new KsefInvoicePage([], false));
        api.Setup(x => x.DownloadInvoiceXmlAsync(42, "number", ct))
            .ReturnsAsync("<Faktura><Fa><P_2>1</P_2></Fa></Faktura>");

        await new GetIncomingKsefInvoicesHandler(api.Object).Handle(new GetIncomingKsefInvoicesRequest { UserId = 42 }, ct);
        await new GetKsefInvoiceDetailsHandler(api.Object).Handle(new GetKsefInvoiceDetailsRequest("number") { UserId = 42 }, ct);
        await new GetKsefInvoiceXmlHandler(api.Object).Handle(new GetKsefInvoiceXmlRequest("number") { UserId = 42 }, ct);
        api.Verify(x => x.DownloadInvoiceXmlAsync(42, "number", ct), Times.Exactly(2));
    }

    [Fact]
    public async Task Sending_without_credentials_does_not_enqueue_invoice()
    {
        var ct = TestContext.Current.CancellationToken;
        var store = new Mock<IKsefCredentialsStore>();
        var outbox = new Mock<IKsefOutboxRepository>(MockBehavior.Strict);
        store.Setup(x => x.GetRequiredAsync(42, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Brak konfiguracji."));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new SendInvoiceToKsefHandler(outbox.Object, store.Object)
                .Handle(new SendInvoiceToKsefRequest(1) { UserId = 42 }, ct));
        outbox.Verify(x => x.EnqueueAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Worker_uses_each_queued_users_credentials_and_waits_for_missing_configuration()
    {
        var ct = TestContext.Current.CancellationToken;
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var outbox = new Mock<IKsefOutboxRepository>(MockBehavior.Strict);
        var store = new Mock<IKsefCredentialsStore>(MockBehavior.Strict);
        var xml = new Mock<IKsefInvoiceXmlGenerator>(MockBehavior.Strict);
        var api = new Mock<IKsefApiClient>(MockBehavior.Strict);
        outbox.Setup(x => x.GetPendingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new KsefOutboxItem(1, 10, 100), new KsefOutboxItem(2, 20, 200), new KsefOutboxItem(3, 30, 300)]);
        outbox.Setup(x => x.MarkInProgressAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        outbox.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        outbox.Setup(x => x.MarkSucceededAsync(It.IsAny<int>(), It.IsAny<KsefSendResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        store.Setup(x => x.IsConfiguredAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        store.Setup(x => x.IsConfiguredAsync(20, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        store.Setup(x => x.IsConfiguredAsync(30, It.IsAny<CancellationToken>()))
            .Callback(() => done.TrySetResult()).ReturnsAsync(false);
        store.Setup(x => x.GetRequiredAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(new KsefCredentials("1234567890", "token-10"));
        store.Setup(x => x.GetRequiredAsync(20, It.IsAny<CancellationToken>())).ReturnsAsync(new KsefCredentials("9876543210", "token-20"));
        xml.Setup(x => x.GenerateXmlAsync(10, 100, It.IsAny<CancellationToken>())).ReturnsAsync("xml-10");
        xml.Setup(x => x.GenerateXmlAsync(20, 200, It.IsAny<CancellationToken>())).ReturnsAsync("xml-20");
        var result = new KsefSendResult("session", "invoice", "ksef", "upo");
        api.Setup(x => x.SendInvoiceAsync(10, "xml-10", It.IsAny<CancellationToken>())).ReturnsAsync(result);
        api.Setup(x => x.SendInvoiceAsync(20, "xml-20", It.IsAny<CancellationToken>())).ReturnsAsync(result);
        using var services = new ServiceCollection()
            .AddSingleton(outbox.Object).AddSingleton(store.Object).AddSingleton(xml.Object).AddSingleton(api.Object)
            .BuildServiceProvider();
        using var worker = new KsefSendingBackgroundService(
            services.GetRequiredService<IServiceScopeFactory>(), NullLogger<KsefSendingBackgroundService>.Instance);

        await worker.StartAsync(ct);
        await done.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);
        await worker.StopAsync(ct);

        api.Verify(x => x.SendInvoiceAsync(10, "xml-10", It.IsAny<CancellationToken>()), Times.Once);
        api.Verify(x => x.SendInvoiceAsync(20, "xml-20", It.IsAny<CancellationToken>()), Times.Once);
        outbox.Verify(x => x.MarkInProgressAsync(3, It.IsAny<CancellationToken>()), Times.Never);
        outbox.Verify(x => x.MarkFailedAsync(3, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

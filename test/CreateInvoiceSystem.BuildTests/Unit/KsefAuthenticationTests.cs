using CreateInvoiceSystem.API.RestServices;
using CreateInvoiceSystem.API.Repositories.KsefCredentialsRepository;
using CreateInvoiceSystem.Modules.Ksef.Dto;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Shared.Persistence;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Interfaces.Services;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class KsefAuthenticationTests
{
    [Fact]
    public async Task Stored_user_credentials_are_passed_unchanged_to_authentication()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(ct);
        var options = new DbContextOptionsBuilder<CreateInvoiceSystemDbContext>().UseSqlite(connection).Options;
        await using var db = new CreateInvoiceSystemDbContext(options);
        await db.Database.EnsureCreatedAsync(ct);
        var user = new UserEntity { UserName = "first", Nip = "1234567890", Address = new AddressEntity() };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var credentials = new KsefCredentialsRepository(db, new EphemeralDataProtectionProvider());
        await credentials.SaveAsync(user.Id, "test-token", ct);
        var authorization = new Mock<IAuthorizationClient>(MockBehavior.Strict);
        var crypto = new Mock<ICryptographyService>(MockBehavior.Strict);
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1234567890000);
        authorization.Setup(x => x.GetAuthChallengeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticationChallengeResponse
            {
                Challenge = "00000000-0000-0000-0000-000000000000",
                Timestamp = timestamp
            });
        crypto.Setup(x => x.EncryptKsefTokenWithRSAUsingPublicKey(
                It.Is<byte[]>(bytes => Encoding.UTF8.GetString(bytes) == "test-token|1234567890000")))
            .Returns([1]);
        authorization.Setup(x => x.SubmitKsefTokenAuthRequestAsync(
                It.Is<AuthenticationKsefTokenRequest>(request =>
                    request.ContextIdentifier.Value == "1234567890" &&
                    request.ContextIdentifier.Type == AuthenticationTokenContextIdentifierType.Nip &&
                    request.EncryptedToken == "AQ=="),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SignatureResponse
            {
                ReferenceNumber = "reference",
                AuthenticationToken = new OperationToken { Token = "temporary" }
            });
        authorization.Setup(x => x.GetAuthStatusAsync("reference", "temporary", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthStatus { Status = new OperationStatusInfo { Code = 450, Description = "failure" } });
        var client = new KsefApiClient(authorization.Object, Mock.Of<IInvoiceDownloadClient>(),
            Mock.Of<IKSeFClient>(), crypto.Object, credentials);

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.DownloadInvoiceXmlAsync(user.Id, "invoice", ct));

        crypto.VerifyAll();
        authorization.VerifyAll();
    }

    [Theory]
    [InlineData(415)]
    [InlineData(450)]
    [InlineData(500)]
    public async Task Authentication_failure_is_reported_without_polling_again(int code)
    {
        var (client, authorization, download) = CreateClient();
        authorization.Setup(x => x.GetAuthStatusAsync("reference", "temporary", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthStatus { Status = new OperationStatusInfo { Code = code, Description = "Authentication failed" } });

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.DownloadInvoiceXmlAsync(1, "invoice", TestContext.Current.CancellationToken));

        Assert.Contains(code.ToString(), error.Message);
        authorization.Verify(x => x.GetAuthStatusAsync("reference", "temporary", It.IsAny<CancellationToken>()), Times.Once);
        authorization.Verify(x => x.GetAccessTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        download.Verify(x => x.GetInvoiceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Pending_authentication_is_polled_until_success()
    {
        var (client, authorization, download) = CreateClient();
        authorization.SetupSequence(x => x.GetAuthStatusAsync("reference", "temporary", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthStatus { Status = new OperationStatusInfo { Code = 100 } })
            .ReturnsAsync(new AuthStatus { Status = new OperationStatusInfo { Code = 200 } });
        authorization.Setup(x => x.GetAccessTokenAsync("temporary", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticationOperationStatusResponse { AccessToken = new TokenInfo { Token = "access" } });
        download.Setup(x => x.GetInvoiceAsync("invoice", "access", It.IsAny<CancellationToken>()))
            .ReturnsAsync("<invoice />");

        var result = await client.DownloadInvoiceXmlAsync(1, "invoice", TestContext.Current.CancellationToken);

        Assert.Equal("<invoice />", result);
        authorization.Verify(x => x.GetAuthStatusAsync("reference", "temporary", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Caller_cancellation_stops_pending_authentication()
    {
        var (client, authorization, _) = CreateClient();
        using var cancellation = new CancellationTokenSource();
        authorization.Setup(x => x.GetAuthStatusAsync("reference", "temporary", It.IsAny<CancellationToken>()))
            .Callback(() => cancellation.Cancel())
            .ReturnsAsync(new AuthStatus { Status = new OperationStatusInfo { Code = 100 } });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.DownloadInvoiceXmlAsync(1, "invoice", cancellation.Token));
        authorization.Verify(x => x.GetAuthStatusAsync("reference", "temporary", It.IsAny<CancellationToken>()), Times.Once);
    }

    private static (KsefApiClient Client, Mock<IAuthorizationClient> Authorization, Mock<IInvoiceDownloadClient> Download) CreateClient()
    {
        var authorization = new Mock<IAuthorizationClient>(MockBehavior.Strict);
        var download = new Mock<IInvoiceDownloadClient>(MockBehavior.Strict);
        var crypto = new Mock<ICryptographyService>(MockBehavior.Strict);
        var credentials = new Mock<IKsefCredentialsStore>(MockBehavior.Strict);
        credentials.Setup(x => x.GetRequiredAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KsefCredentials("1234567890", "test-token"));
        authorization.Setup(x => x.GetAuthChallengeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticationChallengeResponse { Challenge = "00000000-0000-0000-0000-000000000000", Timestamp = DateTimeOffset.UtcNow });
        crypto.Setup(x => x.EncryptKsefTokenWithRSAUsingPublicKey(It.IsAny<byte[]>())).Returns([1]);
        authorization.Setup(x => x.SubmitKsefTokenAuthRequestAsync(It.IsAny<AuthenticationKsefTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SignatureResponse
            {
                ReferenceNumber = "reference",
                AuthenticationToken = new OperationToken { Token = "temporary" }
            });

        return (new KsefApiClient(authorization.Object, download.Object, Mock.Of<IKSeFClient>(), crypto.Object, credentials.Object),
            authorization, download);
    }
}

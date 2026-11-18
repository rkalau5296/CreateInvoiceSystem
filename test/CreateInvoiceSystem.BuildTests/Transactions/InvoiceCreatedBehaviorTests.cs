using CreateInvoiceSystem.API.InvoiceCreatedBehavior;
using CreateInvoiceSystem.API.Mappers.InvoiceMapper;
using CreateInvoiceSystem.API.TransactionBehavior;
using CreateInvoiceSystem.Messaging.Contracts;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.CreateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Dto;
using CreateInvoiceSystem.Modules.Invoices.Entities;
using CreateInvoiceSystem.Modules.Invoices.Mappers;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Shared.Persistence;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace CreateInvoiceSystem.BuildTests.Transactions;

public class InvoiceCreatedBehaviorTests(SqlServerContainerFixture fixture)
    : IClassFixture<SqlServerContainerFixture>
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InvoiceAndOutboxMessageAreCommittedOrRolledBackTogether(bool failAfterOutbox)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddDbContext<CreateInvoiceSystemDbContext>(options =>
                options.UseSqlServer(fixture.ConnectionString))
            .AddSingleton<OutboxProbeState>()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<OutboxProbeConsumer>();
                x.AddEntityFrameworkOutbox<CreateInvoiceSystemDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.QueryDelay = TimeSpan.FromSeconds(1);
                    o.UseBusOutbox();
                });
            })
            .Configure<TestHarnessOptions>(options =>
                options.TestInactivityTimeout = TimeSpan.FromSeconds(15))
            .BuildServiceProvider(true);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var user = new UserEntity
        {
            UserName = $"test-{Guid.NewGuid():N}",
            Nip = Guid.NewGuid().ToString("N"),
            Address = new AddressEntity()
        };
        context.Users.Add(user);
        await context.SaveChangesAsync(ct);

        var invoice = new Invoice
        {
            Title = $"test-{Guid.NewGuid():N}",
            UserId = user.Id,
            CreatedDate = DateTime.UtcNow,
            PaymentDate = DateTime.UtcNow.AddDays(7),
            MethodOfPayment = "Transfer"
        };
        var request = new CreateInvoiceRequest(new CreateInvoiceDto { UserId = user.Id })
        {
            UserId = user.Id
        };
        RequestHandlerDelegate<CreateInvoiceResponse> handler = _ =>
        {
            context.Invoices.Add(InvoiceMapper.ToInvoiceEntity(invoice));
            return Task.FromResult(new CreateInvoiceResponse { Data = invoice.ToDto() });
        };

        var transaction = new TransactionBehavior<CreateInvoiceRequest, CreateInvoiceResponse>(
            context,
            Mock.Of<ILogger<TransactionBehavior<CreateInvoiceRequest, CreateInvoiceResponse>>>());
        var behavior = new InvoiceCreatedBehavior(context, publisher);
        RequestHandlerDelegate<CreateInvoiceResponse> behaviorStep = async _ =>
        {
            var response = await behavior.Handle(request, handler, ct);
            if (failAfterOutbox)
            {
                await context.SaveChangesAsync(ct);
                throw new InvalidOperationException("Simulated failure before commit");
            }

            return response;
        };

        CreateInvoiceResponse? result = null;
        if (failAfterOutbox)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                transaction.Handle(request, behaviorStep, ct));
        }
        else
        {
            result = await transaction.Handle(request, behaviorStep, ct);
        }

        await using var verificationScope = provider.CreateAsyncScope();
        var verification = verificationScope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();
        var savedInvoice = await verification.Invoices.SingleOrDefaultAsync(
            i => i.UserId == user.Id && i.Title == invoice.Title, ct);
        var allMessages = await verification.Set<OutboxMessage>()
            .Where(m => m.MessageType.Contains("InvoiceCreatedIntegrationEvent"))
            .ToListAsync(ct);
        var messages = allMessages.Where(message =>
        {
            using var body = JsonDocument.Parse(message.Body);
            return body.RootElement.GetProperty("message").GetProperty("userId").GetInt32() == user.Id;
        }).ToList();

        if (failAfterOutbox)
        {
            Assert.Null(savedInvoice);
            Assert.Empty(messages);
        }
        else
        {
            Assert.NotNull(savedInvoice);
            Assert.Equal(savedInvoice.InvoiceId, result!.Data.InvoiceId);
            var message = Assert.Single(messages);
            using var body = JsonDocument.Parse(message.Body);
            var eventData = body.RootElement.GetProperty("message");
            Assert.Equal(savedInvoice.InvoiceId, eventData.GetProperty("invoiceId").GetInt32());
            Assert.Equal(user.Id, eventData.GetProperty("userId").GetInt32());

            await scope.DisposeAsync();
            var harness = provider.GetTestHarness();
            await harness.Start();
            var received = await provider.GetRequiredService<OutboxProbeState>()
                .Received.Task.WaitAsync(TimeSpan.FromSeconds(15), ct);
            Assert.Equal(savedInvoice.InvoiceId, received.InvoiceId);
            Assert.Equal(user.Id, received.UserId);
        }
    }
}

public sealed class OutboxProbeState
{
    public TaskCompletionSource<InvoiceCreatedIntegrationEvent> Received { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}

public sealed class OutboxProbeConsumer(OutboxProbeState state) : IConsumer<InvoiceCreatedIntegrationEvent>
{
    public Task Consume(ConsumeContext<InvoiceCreatedIntegrationEvent> context)
    {
        state.Received.TrySetResult(context.Message);
        return Task.CompletedTask;
    }
}

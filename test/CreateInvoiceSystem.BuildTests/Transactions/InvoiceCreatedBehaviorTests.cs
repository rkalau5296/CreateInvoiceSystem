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
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Transactions;

public class InvoiceCreatedBehaviorTests(SqlServerContainerFixture fixture)
    : IClassFixture<SqlServerContainerFixture>
{
    private CreateInvoiceSystemDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CreateInvoiceSystemDbContext>()
            .UseSqlServer(fixture.ConnectionString)
            .Options;

        return new CreateInvoiceSystemDbContext(options);
    }

    [Fact]
    public async Task PublishesInvoiceAfterCommit()
    {
        await using var context = CreateContext();
        var ct = TestContext.Current.CancellationToken;

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

        var publisher = new Mock<IPublishEndpoint>();

        publisher
            .Setup(p => p.Publish(
                It.IsAny<InvoiceCreatedIntegrationEvent>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (InvoiceCreatedIntegrationEvent message, CancellationToken token) =>
            {
                await using var verification = CreateContext();

                Assert.True(await verification.Invoices.AnyAsync(
                    i => i.InvoiceId == message.InvoiceId && i.UserId == user.Id,
                    token));
            });

        var request = new CreateInvoiceRequest(
            new CreateInvoiceDto { UserId = user.Id })
        {
            UserId = user.Id
        };

        RequestHandlerDelegate<CreateInvoiceResponse> handler = _ =>
        {
            context.Invoices.Add(InvoiceMapper.ToInvoiceEntity(invoice));

            return Task.FromResult(new CreateInvoiceResponse
            {
                Data = invoice.ToDto()
            });
        };

        var transaction = new TransactionBehavior<CreateInvoiceRequest, CreateInvoiceResponse>(
            context,
            Mock.Of<ILogger<TransactionBehavior<CreateInvoiceRequest, CreateInvoiceResponse>>>());

        var behavior = new InvoiceCreatedBehavior(context, publisher.Object);

        RequestHandlerDelegate<CreateInvoiceResponse> transactionStep = _ =>
            transaction.Handle(request, handler, ct);

        var result = await behavior.Handle(request, transactionStep, ct);

        Assert.True(result.Data.InvoiceId > 0);
        publisher.Verify(
            p => p.Publish(
                It.Is<InvoiceCreatedIntegrationEvent>(message =>
                    message.InvoiceId == result.Data.InvoiceId &&
                    message.UserId == user.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DoesNotPublishWhenTransactionRollsBack()
    {
        await using var context = CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var (user, invoice) = await CreateInvoiceAsync(context, ct);
        var publisher = new Mock<IPublishEndpoint>();
        var request = new CreateInvoiceRequest(new CreateInvoiceDto { UserId = user.Id })
        {
            UserId = user.Id
        };

        RequestHandlerDelegate<CreateInvoiceResponse> handler = async _ =>
        {
            context.Invoices.Add(InvoiceMapper.ToInvoiceEntity(invoice));
            await context.SaveChangesAsync(ct);
            throw new InvalidOperationException("Handler failed");
        };

        var transaction = new TransactionBehavior<CreateInvoiceRequest, CreateInvoiceResponse>(
            context,
            Mock.Of<ILogger<TransactionBehavior<CreateInvoiceRequest, CreateInvoiceResponse>>>());
        var behavior = new InvoiceCreatedBehavior(context, publisher.Object);
        RequestHandlerDelegate<CreateInvoiceResponse> transactionStep = _ =>
            transaction.Handle(request, handler, ct);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(request, transactionStep, ct));

        await using var verification = CreateContext();
        Assert.False(await verification.Invoices.AnyAsync(
            i => i.UserId == user.Id && i.Title == invoice.Title, ct));
        publisher.Verify(
            p => p.Publish(
                It.IsAny<InvoiceCreatedIntegrationEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task KeepsCommittedInvoiceWhenPublishFails()
    {
        await using var context = CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var (user, invoice) = await CreateInvoiceAsync(context, ct);
        var publisher = new Mock<IPublishEndpoint>();
        publisher
            .Setup(p => p.Publish(
                It.IsAny<InvoiceCreatedIntegrationEvent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Publish failed"));

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
        var behavior = new InvoiceCreatedBehavior(context, publisher.Object);
        RequestHandlerDelegate<CreateInvoiceResponse> transactionStep = _ =>
            transaction.Handle(request, handler, ct);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(request, transactionStep, ct));

        await using var verification = CreateContext();
        Assert.True(await verification.Invoices.AnyAsync(
            i => i.UserId == user.Id && i.Title == invoice.Title, ct));
        publisher.Verify(
            p => p.Publish(
                It.IsAny<InvoiceCreatedIntegrationEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static async Task<(UserEntity User, Invoice Invoice)> CreateInvoiceAsync(
        CreateInvoiceSystemDbContext context,
        CancellationToken ct)
    {
        var user = new UserEntity
        {
            UserName = $"test-{Guid.NewGuid():N}",
            Nip = Guid.NewGuid().ToString("N"),
            Address = new AddressEntity()
        };
        context.Users.Add(user);
        await context.SaveChangesAsync(ct);

        return (user, new Invoice
        {
            Title = $"test-{Guid.NewGuid():N}",
            UserId = user.Id,
            CreatedDate = DateTime.UtcNow,
            PaymentDate = DateTime.UtcNow.AddDays(7),
            MethodOfPayment = "Transfer"
        });
    }
}
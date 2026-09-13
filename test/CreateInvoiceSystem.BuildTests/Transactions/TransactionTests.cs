using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.API.TransactionBehavior;
using CreateInvoiceSystem.Invoices.Persistence.Shared.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Transactions;

public sealed class TransactionTests
    : IClassFixture<SqlServerContainerFixture>
{
    private readonly DbContextOptions<CreateInvoiceSystemDbContext> _options;

    public TransactionTests(SqlServerContainerFixture fixture)
    {
        _options = new DbContextOptionsBuilder<
            CreateInvoiceSystemDbContext>()
            .UseSqlServer(fixture.ConnectionString)
            .Options;
    }

    [Fact]
    public async Task ShouldCommitChanges_WhenPipelineCompletes()
    {
        var invoiceTitle =
            $"Committed invoice {Guid.NewGuid():N}";

        await using var context =
            new CreateInvoiceSystemDbContext(_options);

        var user = await CreateUserAsync(context);

        var loggerMock =
            new Mock<ILogger<TransactionBehavior<TestCommand, bool>>>();

        var behavior =
            new TransactionBehavior<TestCommand, bool>(
                context,
                loggerMock.Object);

        var command = new TestCommand(invoiceTitle);

        RequestHandlerDelegate<bool> next = _ =>
        {
            context.Invoices.Add(new InvoiceEntity
            {
                Title = invoiceTitle,
                UserId = user.Id,
                CreatedDate = DateTime.UtcNow,
                PaymentDate = DateTime.UtcNow.AddDays(7),
                SellerName = "Test Seller",
                SellerNip = "123",
                SellerAddress = "Seller address",
                BankAccountNumber = "123456789",
                ClientName = "Test Client",
                ClientNip = "456",
                ClientAddress = "Client address",
                MethodOfPayment = "Transfer",
                Comments = "Test comments"
            });

            return Task.FromResult(true);
        };

        var result = await behavior.Handle(
            command,
            next,
            CancellationToken.None);

        Assert.True(result);

        await using var verificationContext =
            new CreateInvoiceSystemDbContext(_options);

        var invoiceExists = await verificationContext.Invoices
            .AnyAsync(invoice =>
                invoice.Title == invoiceTitle
                && invoice.UserId == user.Id);

        Assert.True(invoiceExists);
    }

    [Fact]
    public async Task ShouldRollbackAllChanges_WhenExceptionOccursInPipeline()
    {
        var invoiceTitle =
            $"Rolled back invoice {Guid.NewGuid():N}";

        await using var context =
            new CreateInvoiceSystemDbContext(_options);

        var user = await CreateUserAsync(context);

        var loggerMock =
            new Mock<ILogger<TransactionBehavior<TestCommand, bool>>>();

        var behavior =
            new TransactionBehavior<TestCommand, bool>(
                context,
                loggerMock.Object);

        var command = new TestCommand(invoiceTitle);

        RequestHandlerDelegate<bool> next = async _ =>
        {
            var address = new AddressEntity
            {
                Street = "Rollback Street",
                Number = "1",
                City = "Test City",
                PostalCode = "00-000",
                Country = "Poland"
            };

            context.Addresses.Add(address);

            var client = new ClientEntity
            {
                Name = $"Rollback Client {Guid.NewGuid():N}",
                Nip = CreateNip(),
                Email = $"rollback_{Guid.NewGuid():N}@test.local",
                UserId = user.Id,
                Address = address
            };

            context.Clients.Add(client);

            var product = new ProductEntity
            {
                Name = $"Rollback Product {Guid.NewGuid():N}",
                Description = "Rollback description",
                Value = 100m,
                UserId = user.Id
            };

            context.Products.Add(product);

            var invoice = new InvoiceEntity
            {
                Title = invoiceTitle,
                UserId = user.Id,
                CreatedDate = DateTime.UtcNow,
                PaymentDate = DateTime.UtcNow.AddDays(7),
                SellerName = "Test Seller",
                SellerNip = "123",
                SellerAddress = "Seller address",
                BankAccountNumber = "123456789",
                ClientName = client.Name,
                ClientNip = client.Nip,
                ClientAddress = "Client address",
                ClientEmail = client.Email,
                MethodOfPayment = "Transfer",
                Comments = "Rollback invoice"                
            };

            invoice.InvoicePositions.Add(
                new InvoicePositionEntity
                {
                    ProductName = product.Name,
                    ProductDescription = product.Description,
                    ProductValue = product.Value,
                    Quantity = 1,
                    VatRate = "23%"                    
                });

            context.Invoices.Add(invoice);

            await context.SaveChangesAsync();

            throw new InvalidOperationException(
                "Simulated transaction failure.");
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(
                command,
                next,
                CancellationToken.None));

        await using var verificationContext =
            new CreateInvoiceSystemDbContext(_options);

        var invoiceExists = await verificationContext.Invoices
            .AnyAsync(invoice =>
                invoice.Title == invoiceTitle);

        var clientExists = await verificationContext.Clients
            .AnyAsync(client =>
                client.Name.StartsWith("Rollback Client "));

        var productExists = await verificationContext.Products
            .AnyAsync(product =>
                product.Name.StartsWith("Rollback Product "));

        var positionExists = await verificationContext.InvoicePositions
            .AnyAsync(position =>
                position.Invoice.Title == invoiceTitle);

        Assert.False(invoiceExists);
        Assert.False(clientExists);
        Assert.False(productExists);
        Assert.False(positionExists);
    }

    private static async Task<UserEntity> CreateUserAsync(
        CreateInvoiceSystemDbContext context)
    {
        var address = new AddressEntity
        {
            Street = "User Street",
            Number = "1",
            City = "Test City",
            PostalCode = "00-000",
            Country = "Poland"
        };

        context.Addresses.Add(address);
        await context.SaveChangesAsync();

        var user = new UserEntity
        {
            UserName = $"user_{Guid.NewGuid():N}@test.local",
            Email = $"user_{Guid.NewGuid():N}@test.local",
            Name = "Test User",
            CompanyName = "Test Company",
            Nip = CreateNip(),
            IsActive = true,
            EmailConfirmed = true,
            AddressId = address.AddressId
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user;
    }

    private static string CreateNip()
    {
        return Random.Shared
            .NextInt64(1000000000, 9999999999)
            .ToString();
    }
}

public sealed record TestCommand(string Title)
    : IRequest<bool>, ITransactionalRequest;
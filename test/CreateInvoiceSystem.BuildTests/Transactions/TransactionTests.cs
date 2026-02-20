using CreateInvoiceSystem.API.TransactionBehavior;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Transactions;

public class TransactionTests
{
    private readonly DbContextOptions<CreateInvoiceSystemDbContext> _options;

    public TransactionTests()
    {
        _options = new DbContextOptionsBuilder<CreateInvoiceSystemDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=CreateInvoiceSystem_Tests;Trusted_Connection=True;")
            .Options;

        using var context = new CreateInvoiceSystemDbContext(_options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Handle_ShouldRollbackTransaction_WhenExceptionOccursInPipeline()
    {
        using var context = new CreateInvoiceSystemDbContext(_options);

        var testAddress = new AddressEntity
        {
            City = "Test City",
            Street = "Test Street",
            Number = "123",
            PostalCode = "00-000",
            Country = "Poland"
        };
        context.Addresses.Add(testAddress);
        await context.SaveChangesAsync();

        var testUser = new UserEntity
        {
            UserName = "test_user_" + Guid.NewGuid(),
            Email = "test@example.com",
            Nip = Guid.NewGuid().ToString().Substring(0, 10),
            AddressId = testAddress.AddressId
        };
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<TransactionBehavior<TestCommand, bool>>>();
        var behavior = new TransactionBehavior<TestCommand, bool>(context, loggerMock.Object);
        var command = new TestCommand("Faktura Testowa");

        RequestHandlerDelegate<bool> next = async (_) =>
        {
            context.Invoices.Add(new InvoiceEntity
            {
                Title = command.Title,
                UserId = testUser.Id,
                CreatedDate = DateTime.Now,
                PaymentDate = DateTime.Now.AddDays(7),
                SellerName = "Test Seller",
                ClientName = "Test Client",
                MethodOfPayment = "Transfer",
                SellerNip = "123",
                ClientNip = "456",
                SellerAddress = "Address 1",
                ClientAddress = "Address 2",
                BankAccountNumber = "123456789",
                Comments = "Test comments"
            });

            await context.SaveChangesAsync();

            throw new InvalidOperationException("Simulated error");
            return true;
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(command, next, CancellationToken.None));

        using var checkContext = new CreateInvoiceSystemDbContext(_options);
        var count = await checkContext.Invoices.CountAsync();

        Assert.Equal(0, count);
    }
}

public record TestCommand(string Title) : IRequest<bool>;
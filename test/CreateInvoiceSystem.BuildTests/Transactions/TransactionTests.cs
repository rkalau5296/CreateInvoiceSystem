using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.API.TransactionBehavior;
using CreateInvoiceSystem.API.Repositories.ClientRepository;
using CreateInvoiceSystem.API.Repositories.InvoiceRepository;
using CreateInvoiceSystem.API.Repositories.KsefCredentialsRepository;
using CreateInvoiceSystem.API.Repositories.KsefOutboxRepository;
using CreateInvoiceSystem.API.Repositories.ProductRepository;
using CreateInvoiceSystem.API.Repositories.UserRepository;
using CreateInvoiceSystem.Abstractions.Notification;
using CreateInvoiceSystem.Invoices.Persistence.Shared.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Ksef.Persistence.Entities;
using CreateInvoiceSystem.Modules.Ksef.Application.Handlers;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.KsefSettings;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.SendInvoiceToKsef;
using CreateInvoiceSystem.Modules.Invoices.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.DeleteInvoice;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.DeleteUser;
using CreateInvoiceSystem.Modules.Users.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Shared.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MediatorUnit = MediatR.Unit;

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
    public async Task Enqueue_and_delete_are_serialized_for_the_same_invoice()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var setup = new CreateInvoiceSystemDbContext(_options);
        var user = await CreateUserAsync(setup, ct);
        var invoice = new InvoiceEntity
        {
            UserId = user.Id, Title = "Concurrent KSeF", CreatedDate = DateTime.UtcNow,
            PaymentDate = DateTime.UtcNow.AddDays(1)
        };
        setup.Invoices.Add(invoice);
        await setup.SaveChangesAsync(ct);

        await using var sendDb = new CreateInvoiceSystemDbContext(_options);
        await using var deleteDb = new CreateInvoiceSystemDbContext(_options);
        var sendBehavior = new TransactionBehavior<SendInvoiceToKsefRequest, int>(
            sendDb, Mock.Of<ILogger<TransactionBehavior<SendInvoiceToKsefRequest, int>>>());
        var deleteBehavior = new TransactionBehavior<DeleteInvoiceRequest, DeleteInvoiceResponse>(
            deleteDb, Mock.Of<ILogger<TransactionBehavior<DeleteInvoiceRequest, DeleteInvoiceResponse>>>());
        var sendRequest = new SendInvoiceToKsefRequest(invoice.InvoiceId) { UserId = user.Id };
        var deleteRequest = new DeleteInvoiceRequest(invoice.InvoiceId) { UserId = user.Id };
        var queued = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var send = sendBehavior.Handle(sendRequest, async token =>
        {
            var id = await new KsefOutboxRepository(sendDb, sendDb).EnqueueAsync(user.Id, invoice.InvoiceId, token);
            queued.SetResult();
            await release.Task.WaitAsync(token);
            return id;
        }, ct);
        await queued.Task.WaitAsync(TimeSpan.FromSeconds(10), ct);
        var delete = deleteBehavior.Handle(deleteRequest,
            token => new DeleteInvoiceHandler(new InvoiceRepository(deleteDb)).Handle(deleteRequest, token), ct);

        try
        {
            Assert.NotSame(delete, await Task.WhenAny(delete, Task.Delay(300, ct)));
        }
        finally
        {
            release.TrySetResult();
        }

        await send.WaitAsync(TimeSpan.FromSeconds(10), ct);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => delete.WaitAsync(TimeSpan.FromSeconds(10), ct));
        Assert.Contains("KSeF", exception.Message);
        await using var verify = new CreateInvoiceSystemDbContext(_options);
        Assert.True(await verify.Invoices.AnyAsync(i => i.InvoiceId == invoice.InvoiceId, ct));
        Assert.True(await verify.KsefOutgoingInvoiceStatuses.AnyAsync(s => s.InvoiceId == invoice.InvoiceId, ct));
    }

    [Fact]
    public async Task ShouldCommitChanges_WhenPipelineCompletes()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        var invoiceTitle =
            $"Committed invoice {Guid.NewGuid():N}";

        await using var context =
            new CreateInvoiceSystemDbContext(_options);

        var user = await CreateUserAsync(
            context,
            cancellationToken);

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
            cancellationToken);

        Assert.True(result);

        await using var verificationContext =
            new CreateInvoiceSystemDbContext(_options);

        var invoiceExists = await verificationContext.Invoices
            .AnyAsync(
                invoice =>
                    invoice.Title == invoiceTitle
                    && invoice.UserId == user.Id,
                cancellationToken);

        Assert.True(invoiceExists);
    }

    [Fact]
    public async Task ShouldRollbackAllChanges_WhenExceptionOccursInPipeline()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        var invoiceTitle =
            $"Rolled back invoice {Guid.NewGuid():N}";

        await using var context =
            new CreateInvoiceSystemDbContext(_options);

        var user = await CreateUserAsync(
            context,
            cancellationToken);

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

            await context.SaveChangesAsync(cancellationToken);

            throw new InvalidOperationException(
                "Simulated transaction failure.");
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(
                command,
                next,
                cancellationToken));

        await using var verificationContext =
            new CreateInvoiceSystemDbContext(_options);

        var invoiceExists = await verificationContext.Invoices
            .AnyAsync(
                invoice => invoice.Title == invoiceTitle,
                cancellationToken);

        var clientExists = await verificationContext.Clients
            .AnyAsync(
                client => client.Name.StartsWith("Rollback Client "),
                cancellationToken);

        var productExists = await verificationContext.Products
            .AnyAsync(
                product => product.Name.StartsWith("Rollback Product "),
                cancellationToken);

        var positionExists = await verificationContext.InvoicePositions
            .AnyAsync(
                position => position.Invoice.Title == invoiceTitle,
                cancellationToken);

        Assert.False(invoiceExists);
        Assert.False(clientExists);
        Assert.False(productExists);
        Assert.False(positionExists);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteUser_StagesRelatedDeletesUntilCommit_AndRollsBackOnFailure(bool commit)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var context = new CreateInvoiceSystemDbContext(_options);
        var user = await CreateUserAsync(context, ct);
        var userId = user.Id;
        var userAddressId = user.AddressId;

        var clientAddress = new AddressEntity
        {
            Street = "Client Street", Number = "1", City = "Test City",
            PostalCode = "00-000", Country = "Poland"
        };
        var client = new ClientEntity
        {
            Name = "Delete Client", Nip = CreateNip(), UserId = userId, Address = clientAddress
        };
        var product = new ProductEntity
        {
            Name = "Delete Product", Value = 100m, UserId = userId
        };
        var invoice = new InvoiceEntity
        {
            Title = $"Delete invoice {Guid.NewGuid():N}", UserId = userId,
            CreatedDate = DateTime.UtcNow, PaymentDate = DateTime.UtcNow.AddDays(7),
            SellerName = "Seller", SellerNip = "123", SellerAddress = "Address",
            BankAccountNumber = "123", ClientName = "Client", ClientNip = "456",
            ClientAddress = "Address", MethodOfPayment = "Transfer", Comments = ""
        };
        invoice.InvoicePositions.Add(new InvoicePositionEntity
        {
            ProductName = "Delete Product", ProductValue = 100m, Quantity = 1, VatRate = "23%"
        });
        context.AddRange(client, product, invoice);
        await context.SaveChangesAsync(ct);
        var invoiceId = invoice.InvoiceId;
        var clientAddressId = clientAddress.AddressId;
        context.KsefOutgoingInvoiceStatuses.Add(new KsefOutgoingInvoiceStatus
        {
            InvoiceId = invoiceId, UserId = userId, Status = KsefInvoiceStatus.Zaakceptowana
        });
        await context.SaveChangesAsync(ct);
        context.ChangeTracker.Clear();

        var userManager = new Mock<UserManager<UserEntity>>(
            Mock.Of<IUserStore<UserEntity>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        var userRepository = new UserRepository(context, userManager.Object,
            Mock.Of<IHttpContextAccessor>(), new UpperInvariantLookupNormalizer());
        var clients = new ClientRepository(context);
        var products = new ProductRepository(context);
        var invoices = new InvoiceRepository(context);
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Publish(It.IsAny<UserDeletedNotification>(), It.IsAny<CancellationToken>()))
            .Returns(async (UserDeletedNotification notification, CancellationToken token) =>
            {
                await invoices.RemoveAllByUserIdAsync(notification.UserId, token);
                await clients.RemoveAllByUserIdAsync(notification.UserId, token);
                await products.RemoveAllByUserIdAsync(notification.UserId, token);
            });
        var handler = new DeleteUserHandler(userRepository, mediator.Object);
        var behavior = new TransactionBehavior<DeleteUserRequest, DeleteUserResponse>(
            context, Mock.Of<ILogger<TransactionBehavior<DeleteUserRequest, DeleteUserResponse>>>());
        var request = new DeleteUserRequest(userId);
        RequestHandlerDelegate<DeleteUserResponse> next = async _ =>
        {
            var result = await handler.Handle(request, ct);
            await using var beforeCommit = new CreateInvoiceSystemDbContext(_options);
            Assert.True(await beforeCommit.Users.AnyAsync(u => u.Id == userId, ct));
            Assert.True(await beforeCommit.Invoices.AnyAsync(i => i.InvoiceId == invoiceId, ct));
            if (!commit)
                throw new InvalidOperationException("Simulated failure");
            return result;
        };

        if (commit)
            await behavior.Handle(request, next, ct);
        else
            await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.Handle(request, next, ct));

        await using var verification = new CreateInvoiceSystemDbContext(_options);
        Assert.Equal(!commit, await verification.Users.AnyAsync(u => u.Id == userId, ct));
        Assert.Equal(!commit, await verification.Invoices.AnyAsync(i => i.InvoiceId == invoiceId, ct));
        Assert.Equal(!commit, await verification.KsefOutgoingInvoiceStatuses.AnyAsync(s => s.InvoiceId == invoiceId, ct));
        Assert.Equal(!commit, await verification.Clients.AnyAsync(c => c.UserId == userId, ct));
        Assert.Equal(!commit, await verification.Products.AnyAsync(p => p.UserId == userId, ct));
        Assert.Equal(!commit, await verification.Addresses.AnyAsync(a => a.AddressId == clientAddressId, ct));
        Assert.Equal(!commit, await verification.Addresses.AnyAsync(a => a.AddressId == userAddressId, ct));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UserChangesAndNewSession_AreCommittedTogether(bool commit)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var context = new CreateInvoiceSystemDbContext(_options);
        var entity = await CreateUserAsync(context, ct);
        var userId = entity.Id;
        var originalName = entity.Name;
        context.ChangeTracker.Clear();

        var userManager = new Mock<UserManager<UserEntity>>(
            Mock.Of<IUserStore<UserEntity>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        var repository = new UserRepository(context, userManager.Object,
            Mock.Of<IHttpContextAccessor>(), new UpperInvariantLookupNormalizer());
        var behavior = new TransactionBehavior<TestCommand, bool>(
            context, Mock.Of<ILogger<TransactionBehavior<TestCommand, bool>>>());
        var sessionId = Guid.NewGuid();
        RequestHandlerDelegate<bool> next = async _ =>
        {
            var user = await repository.GetUserByIdAsync(userId, ct);
            user.Name = "Updated Name";
            await repository.UpdateAsync(user, ct);
            await repository.AddSessionAsync(new UserSession
            {
                UserId = userId,
                SessionId = sessionId,
                RefreshToken = Guid.NewGuid(),
                LastActivityAt = DateTime.UtcNow
            }, ct);

            await using var beforeCommit = new CreateInvoiceSystemDbContext(_options);
            Assert.Equal(originalName, await beforeCommit.Users.Where(u => u.Id == userId).Select(u => u.Name).SingleAsync(ct));
            Assert.False(await beforeCommit.UserSessions.AnyAsync(s => s.SessionId == sessionId, ct));
            if (!commit)
                throw new InvalidOperationException("Simulated failure");
            return true;
        };

        if (commit)
            await behavior.Handle(new TestCommand("update"), next, ct);
        else
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => behavior.Handle(new TestCommand("update"), next, ct));

        await using var verification = new CreateInvoiceSystemDbContext(_options);
        Assert.Equal(commit ? "Updated Name" : originalName,
            await verification.Users.Where(u => u.Id == userId).Select(u => u.Name).SingleAsync(ct));
        Assert.Equal(commit, await verification.UserSessions.AnyAsync(s => s.SessionId == sessionId, ct));
    }

    [Fact]
    public async Task ActivationToken_IsNotPersistedUntilSaveChanges()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var context = new CreateInvoiceSystemDbContext(_options);
        var entity = await CreateUserAsync(context, ct);
        var userManager = new Mock<UserManager<UserEntity>>(
            Mock.Of<IUserStore<UserEntity>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        var repository = new UserRepository(context, userManager.Object,
            Mock.Of<IHttpContextAccessor>(), new UpperInvariantLookupNormalizer());

        await repository.SaveActivationTokenJtiAsync(entity.Id, "new-jti", DateTimeOffset.UtcNow.AddMinutes(10), ct);
        await using (var reader = new CreateInvoiceSystemDbContext(_options))
            Assert.Null(await reader.Users.Where(u => u.Id == entity.Id).Select(u => u.ActivationTokenJti).SingleAsync(ct));

        await context.SaveChangesAsync(ct);
        await using var verification = new CreateInvoiceSystemDbContext(_options);
        Assert.Equal("new-jti", await verification.Users.Where(u => u.Id == entity.Id).Select(u => u.ActivationTokenJti).SingleAsync(ct));
    }

    [Fact]
    public async Task Cleanup_RemovalIsPersistedOnlyAfterSaveChanges()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var context = new CreateInvoiceSystemDbContext(_options);
        var entity = await CreateUserAsync(context, ct);
        entity.IsActive = false;
        entity.CreatedAt = DateTime.UtcNow.AddDays(-40);
        await context.SaveChangesAsync(ct);
        var userManager = new Mock<UserManager<UserEntity>>(
            Mock.Of<IUserStore<UserEntity>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        var repository = new UserRepository(context, userManager.Object,
            Mock.Of<IHttpContextAccessor>(), new UpperInvariantLookupNormalizer());

        await repository.RemoveInactiveUsersAsync(DateTime.UtcNow.AddDays(-30), ct);

        await using (var before = new CreateInvoiceSystemDbContext(_options))
            Assert.True(await before.Users.AnyAsync(u => u.Id == entity.Id, ct));

        await context.SaveChangesAsync(ct);

        await using var verification = new CreateInvoiceSystemDbContext(_options);
        Assert.False(await verification.Users.AnyAsync(u => u.Id == entity.Id, ct));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UserActivation_IsCommittedOnlyWhenTransactionSucceeds(bool commit)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var context = new CreateInvoiceSystemDbContext(_options);
        var entity = await CreateUserAsync(context, ct);
        var userId = entity.Id;
        var email = entity.Email!;
        entity.IsActive = false;
        entity.ActivationTokenJti = "activation";
        entity.ActivationTokenExpiry = DateTimeOffset.UtcNow.AddMinutes(10);
        await context.SaveChangesAsync(ct);
        context.ChangeTracker.Clear();

        var userManager = new Mock<UserManager<UserEntity>>(
            Mock.Of<IUserStore<UserEntity>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        var repository = new UserRepository(context, userManager.Object,
            Mock.Of<IHttpContextAccessor>(), new UpperInvariantLookupNormalizer());
        var behavior = new TransactionBehavior<TestCommand, bool>(
            context, Mock.Of<ILogger<TransactionBehavior<TestCommand, bool>>>());
        RequestHandlerDelegate<bool> next = async _ =>
        {
            Assert.True(await repository.ValidateAndActivateUserByTokenAsync(email, "activation", null, ct));
            await using var beforeCommit = new CreateInvoiceSystemDbContext(_options);
            Assert.False(await beforeCommit.Users.Where(u => u.Id == userId).Select(u => u.IsActive).SingleAsync(ct));
            if (!commit)
                throw new InvalidOperationException("Simulated failure");
            return true;
        };

        if (commit)
            await behavior.Handle(new TestCommand("activate"), next, ct);
        else
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => behavior.Handle(new TestCommand("activate"), next, ct));

        await using var verification = new CreateInvoiceSystemDbContext(_options);
        var savedUser = await verification.Users.SingleAsync(u => u.Id == userId, ct);
        Assert.Equal(commit, savedUser.IsActive);
        Assert.Equal(commit ? null : "activation", savedUser.ActivationTokenJti);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task KsefSettings_AreCommittedOnlyWhenTransactionSucceeds(bool commit, bool delete)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var context = new CreateInvoiceSystemDbContext(_options);
        var user = await CreateUserAsync(context, ct);
        var provider = new EphemeralDataProtectionProvider();
        var store = new KsefCredentialsRepository(context, provider);
        await store.SaveAsync(user.Id, "original-token", ct);
        await context.SaveChangesAsync(ct);
        context.ChangeTracker.Clear();

        async Task CheckBeforeCommit()
        {
            await using var reader = new CreateInvoiceSystemDbContext(_options);
            var stored = new KsefCredentialsRepository(reader, provider);
            Assert.Equal("original-token", (await stored.GetRequiredAsync(user.Id, ct)).Token);
            if (!commit)
                throw new InvalidOperationException("Simulated failure");
        }

        if (delete)
        {
            var request = new DeleteKsefSettingsRequest(user.Id);
            var handler = new DeleteKsefSettingsHandler(store);
            var behavior = new TransactionBehavior<DeleteKsefSettingsRequest, MediatorUnit>(
                context, Mock.Of<ILogger<TransactionBehavior<DeleteKsefSettingsRequest, MediatorUnit>>>());
            RequestHandlerDelegate<MediatorUnit> next = async _ =>
            {
                await handler.Handle(request, ct);
                await CheckBeforeCommit();
                return MediatorUnit.Value;
            };
            if (commit)
                await behavior.Handle(request, next, ct);
            else
                await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.Handle(request, next, ct));
        }
        else
        {
            var request = new SaveKsefSettingsRequest(user.Id, "new-token");
            var handler = new SaveKsefSettingsHandler(store);
            var behavior = new TransactionBehavior<SaveKsefSettingsRequest, MediatorUnit>(
                context, Mock.Of<ILogger<TransactionBehavior<SaveKsefSettingsRequest, MediatorUnit>>>());
            RequestHandlerDelegate<MediatorUnit> next = async _ =>
            {
                await handler.Handle(request, ct);
                await CheckBeforeCommit();
                return MediatorUnit.Value;
            };
            if (commit)
                await behavior.Handle(request, next, ct);
            else
                await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.Handle(request, next, ct));
        }

        await using var verification = new CreateInvoiceSystemDbContext(_options);
        var savedStore = new KsefCredentialsRepository(verification, provider);
        if (commit && delete)
            Assert.False(await savedStore.IsConfiguredAsync(user.Id, ct));
        else
            Assert.Equal(commit ? "new-token" : "original-token",
                (await savedStore.GetRequiredAsync(user.Id, ct)).Token);
    }

    private static async Task<UserEntity> CreateUserAsync(
        CreateInvoiceSystemDbContext context,
        CancellationToken cancellationToken)
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

        await context.SaveChangesAsync(cancellationToken);

        var email =
            $"user_{Guid.NewGuid():N}@test.local";

        var user = new UserEntity
        {
            UserName = email,
            Email = email,
            Name = "Test User",
            CompanyName = "Test Company",
            Nip = CreateNip(),
            IsActive = true,
            EmailConfirmed = true,
            AddressId = address.AddressId
        };

        context.Users.Add(user);

        await context.SaveChangesAsync(cancellationToken);

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
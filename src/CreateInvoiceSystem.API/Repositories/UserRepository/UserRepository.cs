using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.API.Repositories.UserRepository;

public class UserRepository : IUserRepository
{
    private readonly IDbContext _db;

    public UserRepository(IDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(User entity, CancellationToken cancellationToken)
    {
        await _db.Set<User>().AddAsync(entity, cancellationToken).AsTask();
    }

    public async Task<User> GetUserByIdAsync(int userId, CancellationToken cancellationToken)
    {
        return await _db.Set<User>().AsNoTracking()
            .Include(c => c.Address)
            .SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);
    }

    public async Task<List<User>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var userBase = await (from user in _db.Set<UserEntity>().AsNoTracking()
                              join addr in _db.Set<AddressEntity>().AsNoTracking()
                              on user.AddressId equals addr.AddressId
                              select new { user, addr })
                          .ToListAsync(cancellationToken);

        var allInvoices = await _db.Set<InvoiceEntity>().AsNoTracking().ToListAsync(cancellationToken);
        var allProducts = await _db.Set<ProductEntity>().AsNoTracking().ToListAsync(cancellationToken);
        var allClients = await _db.Set<ClientEntity>().AsNoTracking().ToListAsync(cancellationToken);
        var allClientAddresses = await _db.Set<AddressEntity>().AsNoTracking().ToListAsync(cancellationToken);
        var allInvoicePositions = await _db.Set<InvoicePositionEntity>().AsNoTracking().ToListAsync(cancellationToken);

        return userBase.Select(x => new User
        {
            UserId = x.user.UserId,
            Name = x.user.Name,
            CompanyName = x.user.CompanyName,
            Email = x.user.Email,
            AddressId = x.user.AddressId,
            Address = new Address
            {
                AddressId = x.addr.AddressId,
                Street = x.addr.Street,
                City = x.addr.City,
                Number = x.addr.Number,
                PostalCode = x.addr.PostalCode,
                Country = x.addr.Country,
            },
            Nip = x.user.Nip,
            Invoices = allInvoices
                    .Where(i => i.UserId == x.user.UserId)
                    .Select(i => new Invoice
                    {
                        InvoiceId = i.InvoiceId,
                        Title = i.Title,
                        TotalAmount = i.TotalAmount,
                        PaymentDate = i.PaymentDate,
                        CreatedDate = i.CreatedDate,
                        Comments = i.Comments,
                        ClientId = i.ClientId,
                        UserId = i.UserId,
                        MethodOfPayment = i.MethodOfPayment,
                        ClientName = i.ClientName,
                        ClientAddress = i.ClientAddress,
                        ClientNip = i.ClientNip,
                        InvoicePositions = allInvoicePositions
                            .Where(ip => ip.InvoiceId == i.InvoiceId)
                            .Select(ip => new InvoicePosition
                            {
                                InvoiceId = ip.InvoiceId,
                                InvoicePositionId = ip.InvoicePositionId,
                                ProductId = ip.ProductId,
                                Quantity = ip.Quantity,
                                ProductDescription = ip.ProductDescription,
                                ProductName = ip.ProductName,
                                ProductValue = ip.ProductValue
                            })
                            .ToList()
                    })
                    .ToList(),
            Products = allProducts
                    .Where(p => p.UserId == x.user.UserId)
                    .Select(p => new Product
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        Description = p.Description,
                        Value = p.Value,
                        UserId = p.UserId
                    })
                    .ToList(),
            Clients = allClients
                    .Where(c => c.UserId == x.user.UserId)
                    .Select(c =>
                    {
                        var clientAddrEntity = allClientAddresses.FirstOrDefault(a => a.AddressId == c.AddressId);

                        return new Client
                        {
                            ClientId = c.ClientId,
                            Name = c.Name,
                            Nip = c.Nip,
                            UserId = c.UserId,
                            Address = clientAddrEntity != null ? new Address
                            {
                                AddressId = clientAddrEntity.AddressId,
                                Street = clientAddrEntity.Street,
                                Number = clientAddrEntity.Number,
                                City = clientAddrEntity.City,
                                PostalCode = clientAddrEntity.PostalCode,
                                Country = clientAddrEntity.Country
                            } : null
                        };
                    })
                    .ToList()
        }).ToList();
    }

    public async Task<bool> IsAddressExists(int addressId, CancellationToken cancellationToken)
    {
        return await _db.Set<Address>()
            .AsNoTracking()
            .AnyAsync(a => a.AddressId == addressId, cancellationToken);
    }

    public async Task<bool> IsUserExists(int userId, CancellationToken cancellationToken)
    {
        return await _db.Set<User>()
            .AsNoTracking()
            .AnyAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task RemoveAddress(Address address, CancellationToken cancellationToken)
    {
        _db.Set<Address>().Remove(address);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(User user, CancellationToken cancellationToken)
    {
        _db.Set<User>().Remove(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}

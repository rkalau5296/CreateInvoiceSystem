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

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        var addressEntity = new AddressEntity
        {
            Street = user.Address.Street,
            Number = user.Address.Number,
            City = user.Address.City,
            PostalCode = user.Address.PostalCode,
            Country = user.Address.Country
        };
        
        
        await _db.Set<AddressEntity>().AddAsync(addressEntity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        user.AddressId = addressEntity.AddressId;

        var userEntity = new UserEntity
        {
            UserId = user.UserId,
            Name = user.Name,
            CompanyName = user.CompanyName,
            Email = user.Email,
            Password = user.Password,
            Nip = user.Nip,
            AddressId = user.AddressId
        };

        await _db.Set<UserEntity>().AddAsync(userEntity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);        
    }

    public async Task<User> GetUserByIdAsync(int userId, CancellationToken cancellationToken)
    {
        var baseData = await (from user in _db.Set<UserEntity>().AsNoTracking()
                              join addr in _db.Set<AddressEntity>().AsNoTracking()
                              on user.AddressId equals addr.AddressId
                              where user.UserId == userId
                              select new { user, addr })
                          .SingleOrDefaultAsync(cancellationToken);
        if (baseData == null) return null;
        var invoices = await _db.Set<InvoiceEntity>()
        .Where(i => i.UserId == userId)
        .AsNoTracking()
        .ToListAsync(cancellationToken);

        var invoiceIds = invoices.Select(i => i.InvoiceId).ToList();
        var invoicePositions = await _db.Set<InvoicePositionEntity>()
            .Where(ip => invoiceIds.Contains(ip.InvoiceId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var products = await _db.Set<ProductEntity>()
            .Where(p => p.UserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var clients = await _db.Set<ClientEntity>()
            .Where(c => c.UserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var clientAddressIds = clients.Select(c => c.AddressId).ToList();
        var clientAddresses = await _db.Set<AddressEntity>()
            .Where(a => clientAddressIds.Contains(a.AddressId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        
        return new User
        {
            UserId = baseData.user.UserId,
            Name = baseData.user.Name,
            CompanyName = baseData.user.CompanyName,
            Email = baseData.user.Email,
            Nip = baseData.user.Nip,
            AddressId = baseData.user.AddressId,
            Address = new Address
            {
                AddressId = baseData.addr.AddressId,
                Street = baseData.addr.Street,
                Number = baseData.addr.Number,
                City = baseData.addr.City,
                PostalCode = baseData.addr.PostalCode,
                Country = baseData.addr.Country,
            },
            Invoices = invoices.Select(i => new Invoice
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
                InvoicePositions = invoicePositions
                    .Where(ip => ip.InvoiceId == i.InvoiceId)
                    .Select(ip => new InvoicePosition
                    {
                        InvoicePositionId = ip.InvoicePositionId,
                        InvoiceId = ip.InvoiceId,
                        ProductId = ip.ProductId,
                        Quantity = ip.Quantity,
                        ProductDescription = ip.ProductDescription,
                        ProductName = ip.ProductName,
                        ProductValue = ip.ProductValue
                    }).ToList()
            }).ToList(),
            Products = products.Select(p => new Product
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Value = p.Value,
                UserId = p.UserId
            }).ToList(),
            Clients = clients.Select(c =>
            {
                var clientAddr = clientAddresses.FirstOrDefault(a => a.AddressId == c.AddressId);
                return new Client
                {
                    ClientId = c.ClientId,
                    Name = c.Name,
                    Nip = c.Nip,
                    UserId = c.UserId,
                    Address = clientAddr != null ? new Address
                    {
                        AddressId = clientAddr.AddressId,
                        Street = clientAddr.Street,
                        Number = clientAddr.Number,
                        City = clientAddr.City,
                        PostalCode = clientAddr.PostalCode,
                        Country = clientAddr.Country
                    } : null
                };
            }).ToList()
        };
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

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        var userEntity = await _db.Set<UserEntity>()
            .SingleOrDefaultAsync(u => u.UserId == user.UserId, cancellationToken);

        var addressEntity = await _db.Set<AddressEntity>()
            .SingleOrDefaultAsync(a => a.AddressId == user.AddressId, cancellationToken);


        if (userEntity == null) return;

        userEntity.Name = user.Name;
        userEntity.CompanyName = user.CompanyName;
        userEntity.Email = user.Email;
        userEntity.Password = user.Password;
        userEntity.Nip = user.Nip;

        if (addressEntity != null && user.Address != null)
        {
            addressEntity.Street = user.Address.Street;
            addressEntity.Number = user.Address.Number;
            addressEntity.City = user.Address.City;
            addressEntity.PostalCode = user.Address.PostalCode;
            addressEntity.Country = user.Address.Country;
        }
    }
}

using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.API.Repositories.ClientRepository;

public class ClientRepository(IDbContext db) : IClientRepository
{
    private readonly IDbContext _db = db;

    public Task<bool> ExistsAsync(string name, string street, string number, string city, string postalCode, string country, int userId, CancellationToken cancellationToken)        
    {
        return _db.Set<Client>().AnyAsync(c =>
            c.Name == name &&
            c.Address != null &&
            c.Address.Street == street &&
            c.Address.Number == number &&
            c.Address.City == city &&
            c.Address.PostalCode == postalCode &&
            c.Address.Country == country &&
            c.UserId == userId, cancellationToken);
    }

    public async Task<Client> GetByIdAsync(int clientId, bool includeAddress, CancellationToken cancellationToken)
    {
        IQueryable<ClientEntity> clientQuery = _db.Set<ClientEntity>()
            .AsNoTracking();

        var client = await clientQuery.SingleOrDefaultAsync(c => c.ClientId == clientId, cancellationToken)
            ?? throw new InvalidOperationException($"Client with ID {clientId} not found.");

        IQueryable<AddressEntity> addressQuery = _db.Set<AddressEntity>()
            .AsNoTracking();

        var address = await addressQuery.SingleOrDefaultAsync(a => a.AddressId == client.AddressId, cancellationToken)
            ?? throw new InvalidOperationException($"Address with ID {client.AddressId} not found.");

        return new Client
        {
            ClientId = client.ClientId,
            Name = client.Name,
            Nip = client.Nip,
            AddressId = client.AddressId,
            UserId = client.UserId,
            Address = new Address
            {
                AddressId = address.AddressId,
                Street = address.Street,
                Number = address.Number,
                City = address.City,
                PostalCode = address.PostalCode,
                Country = address.Country
            },
        };
    }

    public async Task<List<Client>> GetAllAsync(bool includeAddress, bool excludeDeleted, CancellationToken cancellationToken)
    {
        IQueryable<Client> query = _db.Set<Client>().AsNoTracking();

        query = includeAddress
            ? query.Include(c => c.Address)
            : query;

        query = excludeDeleted
            ? query.Where(c => !c.IsDeleted)
            : query;

        return await query.ToListAsync(cancellationToken);
    }

    public Task AddAsync(Client entity, CancellationToken cancellationToken) =>
        _db.Set<Client>().AddAsync(entity, cancellationToken).AsTask();

    public async Task UpdateAsync(Client entity, CancellationToken cancellationToken)
    {
        _db.Set<Client>().Update(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Client entity, CancellationToken cancellationToken)
    {
        _db.Set<Client>().Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAddressAsync(Address address, CancellationToken cancellationToken)
    {
        _db.Set<Address>().Remove(address);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsByIdAsync(int clientId, CancellationToken cancellationToken) =>
        _db.Set<Client>().AsNoTracking().AnyAsync(c => c.ClientId == clientId, cancellationToken);

    public Task<bool> AddressExistsByIdAsync(int addressId, CancellationToken cancellationToken) =>
        _db.Set<Address>().AsNoTracking().AnyAsync(a => a.AddressId == addressId, cancellationToken);    

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _db.SaveChangesAsync(cancellationToken);
}
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.API.Mappers.ClientMapper;
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
        return _db.Set<ClientEntity>()
            .AsNoTracking()
            .AnyAsync(c =>
                c.UserId == userId &&
                c.Name == name &&
                _db.Set<AddressEntity>().Any(a =>
                    a.AddressId == c.AddressId &&
                    a.Street == street &&
                    a.Number == number &&
                    a.City == city &&
                    a.PostalCode == postalCode &&
                    a.Country == country
                ), cancellationToken);
    }

    public async Task<Client> GetByIdAsync(int clientId, int? userId, CancellationToken cancellationToken)
    {
        var query = _db.Set<ClientEntity>().AsNoTracking();

        if (userId.HasValue)
            query = query.Where(c => c.UserId == userId.Value);

        var clientEntity = await query.SingleOrDefaultAsync(c => c.ClientId == clientId, cancellationToken)
            ?? throw new InvalidOperationException($"Client with ID {clientId} not found or access denied.");

        var addressEntity = await _db.Set<AddressEntity>()
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.AddressId == clientEntity.AddressId, cancellationToken)
            ?? throw new InvalidOperationException($"Address with ID {clientEntity.AddressId} not found.");

        return ClientMapper.ToDomain(clientEntity, addressEntity);
    }

    public async Task<PagedResult<Client>> GetAllAsync(int? userId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken)
    {
        var query = _db.Set<ClientEntity>().AsNoTracking();

        if (userId.HasValue)
            query = query.Where(c => c.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(c => c.Name.Contains(searchTerm) || c.Nip.Contains(searchTerm));

        var totalCount = await query.CountAsync(cancellationToken);

        var clients = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var addressIds = clients.Select(c => c.AddressId).Distinct().ToList();

        var addresses = await _db.Set<AddressEntity>()
            .AsNoTracking()
            .Where(a => addressIds.Contains(a.AddressId))
            .ToListAsync(cancellationToken);

        var items = clients.Select(c =>
        {
            var addr = addresses.SingleOrDefault(a => a.AddressId == c.AddressId);
            return ClientMapper.ToDomain(c, addr);
        }).ToList();

        return new PagedResult<Client>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Client> AddAsync(Client entity, CancellationToken cancellationToken)
    {
        var addressEntity = ClientMapper.ToAddressEntity(entity.Address!);
        _db.Set<AddressEntity>().Add(addressEntity);
        await _db.SaveChangesAsync(cancellationToken);

        entity.AddressId = addressEntity.AddressId;

        var clientEntity = ClientMapper.ToClientEntity(entity);
        _db.Set<ClientEntity>().Add(clientEntity);
        await _db.SaveChangesAsync(cancellationToken);

        return ClientMapper.ToDomain(clientEntity, addressEntity);
    }

    public async Task<Client> UpdateAsync(Client entity, CancellationToken cancellationToken)
    {
        var addressEntity = ClientMapper.ToAddressEntity(entity.Address!);
        _db.Set<AddressEntity>().Update(addressEntity);
        await _db.SaveChangesAsync(cancellationToken);

        var clientEntity = ClientMapper.ToClientEntity(entity);
        _db.Set<ClientEntity>().Update(clientEntity);
        await _db.SaveChangesAsync(cancellationToken);

        return ClientMapper.ToDomain(clientEntity, addressEntity);
    }

    public async Task RemoveAsync(int clientId, CancellationToken cancellationToken)
    {
        var clientEntity = await _db.Set<ClientEntity>()
            .SingleOrDefaultAsync(c => c.ClientId == clientId, cancellationToken)
            ?? throw new InvalidOperationException($"Client with ID {clientId} not found.");

        _db.Set<ClientEntity>().Remove(clientEntity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAddressAsync(int addressId, CancellationToken cancellationToken)
    {
        var addressEntity = await _db.Set<AddressEntity>()
            .SingleOrDefaultAsync(a => a.AddressId == addressId, cancellationToken)
            ?? throw new InvalidOperationException($"Address with ID {addressId} not found.");

        _db.Set<AddressEntity>().Remove(addressEntity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsByIdAsync(int clientId, CancellationToken cancellationToken) =>
        _db.Set<ClientEntity>().AsNoTracking().AnyAsync(c => c.ClientId == clientId, cancellationToken);

    public Task<bool> AddressExistsByIdAsync(int addressId, CancellationToken cancellationToken) =>
        _db.Set<AddressEntity>().AsNoTracking().AnyAsync(a => a.AddressId == addressId, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);

    public async Task RemoveAllByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        var clients = await _db.Set<ClientEntity>()
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);

        if (clients.Count == 0) return;

        var addressIds = clients.Select(c => c.AddressId).Distinct().ToList();

        var addresses = await _db.Set<AddressEntity>()
            .Where(a => addressIds.Contains(a.AddressId))
            .ToListAsync(cancellationToken);

        _db.Set<ClientEntity>().RemoveRange(clients);

        if (addresses.Count != 0)
            _db.Set<AddressEntity>().RemoveRange(addresses);
    }
}
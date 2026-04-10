using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.API.Mappers.InvoiceMapper;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.API.Repositories.InvoiceRepository;

public class InvoiceRepository(IDbContext db) : IInvoiceRepository
{
    private readonly IDbContext _db = db;

    public async Task AddClientAsync(Client client, CancellationToken cancellationToken)
    {
        var addressEntity = InvoiceMapper.ToAddressEntity(client.Address!);
        await _db.Set<AddressEntity>().AddAsync(addressEntity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var clientEntity = InvoiceMapper.ToClientEntity(client);
        clientEntity.AddressId = addressEntity.AddressId;
        await _db.Set<ClientEntity>().AddAsync(clientEntity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        client.ClientId = clientEntity.ClientId;
        client.AddressId = addressEntity.AddressId;
    }

    public async Task<Invoice> AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var invoiceEntity = InvoiceMapper.ToInvoiceEntity(invoice);
        await _db.Set<InvoiceEntity>().AddAsync(invoiceEntity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var positionsToSave = InvoiceMapper.ToInvoicePositionEntities(invoice.InvoicePositions, invoiceEntity.InvoiceId);
        await _db.Set<InvoicePositionEntity>().AddRangeAsync(positionsToSave, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var productIds = positionsToSave
            .Select(p => p.ProductId)
            .Where(id => id.HasValue)
            .Cast<int>()
            .Distinct()
            .ToList();

        var productsMap = await _db.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId, cancellationToken);

        return InvoiceMapper.MapDetailed(invoiceEntity, null, null, positionsToSave, productsMap);
    }

    public Task AddInvoicePositionAsync(ICollection<InvoicePosition> invoicePositions, CancellationToken cancellationToken)
    {
        var entities = invoicePositions.Select(ip => InvoiceMapper.ToInvoicePositionEntity(ip, ip.InvoiceId)).ToList();
        return _db.Set<InvoicePositionEntity>().AddRangeAsync(entities, cancellationToken);
    }

    public async Task AddProductAsync(Product product, CancellationToken cancellationToken)
    {
        var entity = InvoiceMapper.ToProductEntity(product);
        await _db.Set<ProductEntity>().AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        product.ProductId = entity.ProductId;
    }

    public async Task<Client?> GetClientAsync(string name, string street, string number, string city, string postalCode, string country, int userId, string email, CancellationToken cancellationToken)
    {
        var clientEntity = await (from client in _db.Set<ClientEntity>().AsNoTracking()
                                  join address in _db.Set<AddressEntity>().AsNoTracking()
                                  on client.AddressId equals address.AddressId
                                  where client.Name == name &&
                                        client.UserId == userId &&
                                        client.Email == email &&
                                        address.Street == street &&
                                        address.Number == number &&
                                        address.City == city &&
                                        address.PostalCode == postalCode &&
                                        address.Country == country
                                  select new { client, address })
                        .FirstOrDefaultAsync(cancellationToken);

        if (clientEntity == null) return null;

        return InvoiceMapper.MapClient(clientEntity.client, clientEntity.address);
    }

    public async Task<Client> GetClientByIdAsync(int? clientId, CancellationToken cancellationToken)
    {
        if (clientId == null)
            throw new ArgumentNullException(nameof(clientId));

        var client = await _db.Set<ClientEntity>()
            .AsNoTracking().SingleOrDefaultAsync(c => c.ClientId == clientId, cancellationToken)
            ?? throw new InvalidOperationException($"Client with ID {clientId} not found.");

        var address = await _db.Set<AddressEntity>()
            .AsNoTracking().SingleOrDefaultAsync(a => a.AddressId == client.AddressId, cancellationToken)
            ?? throw new InvalidOperationException($"Address with ID {client.AddressId} not found.");

        return InvoiceMapper.MapClient(client, address)!;
    }

    public async Task<Invoice> GetInvoiceByIdAsync(int? user, int invoiceId, CancellationToken cancellationToken)
    {
        var invoiceEntity = await _db.Set<InvoiceEntity>()
            .AsNoTracking()
            .SingleOrDefaultAsync(i => i.InvoiceId == invoiceId && i.UserId == user, cancellationToken)
            ?? throw new InvalidOperationException($"Invoice with ID {invoiceId} not found.");

        ClientEntity? clientEntity = null;
        AddressEntity? addressEntity = null;

        if (invoiceEntity.ClientId.HasValue)
        {
            clientEntity = await _db.Set<ClientEntity>()
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.ClientId == invoiceEntity.ClientId, cancellationToken);

            if (clientEntity != null)
            {
                addressEntity = await _db.Set<AddressEntity>()
                    .AsNoTracking()
                    .SingleOrDefaultAsync(a => a.AddressId == clientEntity.AddressId, cancellationToken);
            }
        }

        var invoicePositionsEntity = await _db.Set<InvoicePositionEntity>()
            .AsNoTracking()
            .Where(p => p.InvoiceId == invoiceId)
            .ToListAsync(cancellationToken);

        var productIds = invoicePositionsEntity
            .Select(p => p.ProductId)
            .Where(id => id.HasValue)
            .Cast<int>()
            .Distinct()
            .ToList();

        var productsMap = await _db.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId, cancellationToken);

        return InvoiceMapper.MapDetailed(invoiceEntity, clientEntity, addressEntity, invoicePositionsEntity, productsMap);
    }

    public async Task<PagedResult<Invoice>> GetInvoicesAsync(int? userId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken)
    {
        var query = _db.Set<InvoiceEntity>()
            .AsNoTracking()
            .Where(i => i.UserId == userId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLower();
            query = query.Where(i =>
                i.Title.ToLower().Contains(lowerSearch) ||
                i.ClientName.ToLower().Contains(lowerSearch) ||
                i.ClientNip.ToLower().Contains(lowerSearch));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var invoiceEntities = await query
            .OrderByDescending(i => i.InvoiceId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (invoiceEntities.Count == 0)
            return new PagedResult<Invoice>(new List<Invoice>(), totalCount, pageNumber, pageSize);

        var invoiceIds = invoiceEntities.Select(i => i.InvoiceId).ToList();
        var clientIds = invoiceEntities
                .Select(i => i.ClientId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

        var allPositions = await _db.Set<InvoicePositionEntity>()
            .AsNoTracking()
            .Where(p => invoiceIds.Contains(p.InvoiceId))
            .ToListAsync(cancellationToken);

        var allClients = await _db.Set<ClientEntity>()
            .AsNoTracking()
            .Where(c => clientIds.Contains(c.ClientId))
            .ToListAsync(cancellationToken);

        var positionsLookup = allPositions.ToLookup(p => p.InvoiceId);
        var clientsDict = allClients.ToDictionary(c => c.ClientId);

        var items = invoiceEntities.Select(i =>
        {
            clientsDict.TryGetValue(i.ClientId ?? 0, out var cEntity);
            var positionsForInvoice = positionsLookup[i.InvoiceId];
            return InvoiceMapper.MapSummary(i, positionsForInvoice, cEntity);
        }).ToList();

        return new PagedResult<Invoice>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Product?> GetProductAsync(string name, string description, decimal? value, int userId, CancellationToken cancellationToken)
    {
        var product = await _db.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p =>
                p.Name == name &&
                p.Description == description &&
                p.UserId == userId &&
                p.IsDeleted == false &&
                p.Value.HasValue == value.HasValue)
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null)
            return null;

        return InvoiceMapper.MapProduct(product);
    }

    public async Task<Product?> GetProductByIdAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await _db.Set<ProductEntity>().AsNoTracking().SingleOrDefaultAsync(p => p.ProductId == productId, cancellationToken);

        if (product == null)
            return null;

        return InvoiceMapper.MapProduct(product);
    }

    public Task<bool> InvoiceExistsAsync(int invoiceId, CancellationToken cancellationToken)
    {
        return _db.Set<InvoiceEntity>()
            .AsNoTracking()
            .AnyAsync(i => i.InvoiceId == invoiceId, cancellationToken);
    }

    public Task<bool> InvoicePositionExistsAsync(int invoiceId, CancellationToken cancellationToken) =>
         _db.Set<InvoicePositionEntity>()
        .AsNoTracking()
        .AnyAsync(ip => ip.InvoiceId == invoiceId, cancellationToken);

    public async Task RemoveAsync(Invoice invoice)
    {
        var invoiceEntity = await _db.Set<InvoiceEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId, CancellationToken.None)
            ?? throw new InvalidOperationException($"Invoice with ID {invoice.InvoiceId} not found.");
        _db.Set<InvoiceEntity>().Remove(invoiceEntity);
    }

    public async Task RemoveInvoicePositionsAsync(InvoicePosition invoicePosition)
    {
        var invoicePos = await _db.Set<InvoicePositionEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ip => ip.InvoicePositionId == invoicePosition.InvoicePositionId, CancellationToken.None)
            ?? throw new InvalidOperationException($"InvoicePosition with ID {invoicePosition.InvoicePositionId} not found.");
        _db.Set<InvoicePositionEntity>().Remove(invoicePos);
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        int? finalClientId = invoice.ClientId;

        if ((invoice.ClientId == 0 || invoice.ClientId == null) && invoice.Client != null)
        {
            var addressEntity = InvoiceMapper.ToAddressEntity(invoice.Client.Address!);
            await _db.Set<AddressEntity>().AddAsync(addressEntity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            var newClient = InvoiceMapper.ToClientEntity(invoice.Client);
            newClient.AddressId = addressEntity.AddressId;
            newClient.UserId = invoice.UserId;
            await _db.Set<ClientEntity>().AddAsync(newClient, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            finalClientId = newClient.ClientId;
        }

        var invoiceEntity = InvoiceMapper.ToInvoiceEntity(invoice, finalClientId);
        _db.Set<InvoiceEntity>().Update(invoiceEntity);
        await _db.SaveChangesAsync(cancellationToken);

        var positionsToProcess = invoice.InvoicePositions.Select(ip =>
        {
            var entity = InvoiceMapper.ToInvoicePositionEntity(ip, invoiceEntity.InvoiceId);
            if (entity.ProductId.HasValue == false && ip.ProductId > 0)
                entity.ProductId = ip.ProductId;
            return entity;
        }).ToList();

        foreach (var posEntity in positionsToProcess)
        {
            if (posEntity.InvoicePositionId == 0)
                await _db.Set<InvoicePositionEntity>().AddAsync(posEntity, cancellationToken);
            else
                _db.Set<InvoicePositionEntity>().Update(posEntity);
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveRangeAsync(IEnumerable<InvoicePosition> invoicePositions, CancellationToken cancellationToken)
    {
        var invoicePosIds = invoicePositions.Select(ip => ip.InvoicePositionId).ToList();
        var invoicePosEntities = await _db.Set<InvoicePositionEntity>()
            .Where(ip => invoicePosIds.Contains(ip.InvoicePositionId))
            .ToListAsync(cancellationToken);
        _db.Set<InvoicePositionEntity>().RemoveRange(invoicePosEntities);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);

    public async Task<string> GetUserEmailByIdAsync(int userId, CancellationToken ct)
    {
        return await _db.Set<UserEntity>()
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<User?> GetUserByIdAsync(int userId, CancellationToken ct)
    {
        var result = await (from u in _db.Set<UserEntity>()
                            join a in _db.Set<AddressEntity>() on u.AddressId equals a.AddressId
                            where u.Id == userId
                            select new { u, a })
                    .FirstOrDefaultAsync(ct);

        return InvoiceMapper.Map(result?.u, result?.a);
    }

    public async Task<int> GetInvoicesCountInMonthAsync(int userId, int month, int year, CancellationToken ct)
    {
        return await _db.Set<InvoiceEntity>()
            .Where(i => i.UserId == userId)
            .Where(i => i.CreatedDate.Month == month && i.CreatedDate.Year == year)
            .CountAsync(ct);
    }

    public async Task RemoveAllByUserIdAsync(int userId, CancellationToken ct)
    {
        var invoices = await _db.Set<InvoiceEntity>()
            .Where(i => i.UserId == userId)
            .ToListAsync(ct);

        if (invoices.Count != 0)
        {
            var invoiceIds = invoices.Select(i => i.InvoiceId).ToList();

            var positions = await _db.Set<InvoicePositionEntity>()
                .Where(p => invoiceIds.Contains(p.InvoiceId))
                .ToListAsync(ct);

            if (positions.Count != 0)
            {
                _db.Set<InvoicePositionEntity>().RemoveRange(positions);
            }

            _db.Set<InvoiceEntity>().RemoveRange(invoices);

            await _db.SaveChangesAsync(ct);
        }
    }
}
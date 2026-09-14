using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.API.Mappers.InvoiceMapper;
using CreateInvoiceSystem.Invoices.Persistence.Shared.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.API.Repositories.InvoiceRepository;

public class InvoiceRepository(IDbContext db) : IInvoiceRepository
{
    private readonly IDbContext _db = db;

    public async Task AddClientAsync(Client client, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(client.Address);

        var addressEntity = InvoiceMapper.ToAddressEntity(client.Address);

        var clientEntity = InvoiceMapper.ToClientEntity(client);
        clientEntity.Address = addressEntity;

        await _db.Set<ClientEntity>()
            .AddAsync(clientEntity, cancellationToken);
    }

    public Task<Invoice> AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var invoiceEntity = InvoiceMapper.ToInvoiceEntity(invoice);

        var positionsToSave = invoice.InvoicePositions
            .Select(InvoiceMapper.ToInvoicePositionEntity)
            .ToList();

        foreach (var positionEntity in positionsToSave)
        {
            invoiceEntity.InvoicePositions.Add(positionEntity);
        }

        _db.Set<InvoiceEntity>().Add(invoiceEntity);

        return Task.FromResult(
            InvoiceMapper.MapDetailed(
                invoiceEntity,
                null,
                null,
                positionsToSave,
                new Dictionary<int, ProductEntity>()));
    }

    public Task AddInvoicePositionAsync(ICollection<InvoicePosition> invoicePositions, CancellationToken cancellationToken)
    {
        var entities = invoicePositions.Select(ip => InvoiceMapper.ToInvoicePositionEntity(ip)).ToList();
        return _db.Set<InvoicePositionEntity>().AddRangeAsync(entities, cancellationToken);
    }

    public async Task AddProductAsync(Product product, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(product);

        var entity = InvoiceMapper.ToProductEntity(product);

        await _db.Set<ProductEntity>()
            .AddAsync(entity, cancellationToken);
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

    public async Task RemoveAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var invoiceEntity = await _db
            .Set<InvoiceEntity>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entity => entity.InvoiceId == invoice.InvoiceId,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"Invoice with ID {invoice.InvoiceId} not found.");

        _db.Set<InvoiceEntity>()
            .Remove(invoiceEntity);
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
        var invoiceEntity = InvoiceMapper.ToInvoiceEntity(invoice);

        _db.Set<InvoiceEntity>().Update(invoiceEntity);

        foreach (var position in invoice.InvoicePositions)
        {
            var positionEntity = InvoiceMapper.ToInvoicePositionEntity(position);
            positionEntity.InvoiceId = invoice.InvoiceId;

            if (positionEntity.InvoicePositionId == 0)
            {
                await _db.Set<InvoicePositionEntity>()
                    .AddAsync(positionEntity, cancellationToken);
            }
            else
            {
                _db.Set<InvoicePositionEntity>()
                    .Update(positionEntity);
            }
        }
    }

    public async Task RemoveRangeAsync(IEnumerable<InvoicePosition> invoicePositions, CancellationToken cancellationToken)
    {
        var invoicePositionIds = invoicePositions
            .Select(position => position.InvoicePositionId)
            .ToList();

        if (invoicePositionIds.Count == 0)
        {
            return;
        }

        var invoicePositionEntities = await _db
            .Set<InvoicePositionEntity>()
            .Where(position =>
                invoicePositionIds.Contains(position.InvoicePositionId))
            .ToListAsync(cancellationToken);

        _db.Set<InvoicePositionEntity>()
            .RemoveRange(invoicePositionEntities);
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

    public async Task<int> GetMaxInvoiceNumberInMonthAsync(int userId, int month, int year, CancellationToken ct)
    {
        var titles = await _db.Set<InvoiceEntity>()
            .Where(i => i.UserId == userId)
            .Where(i => i.CreatedDate.Month == month && i.CreatedDate.Year == year)
            .Select(i => i.Title)
            .ToListAsync(ct);

        return titles
            .Select(title => int.TryParse(
                title.Split('/', 2)[0],
                out var number)
                    ? number
                    : 0)
            .DefaultIfEmpty(0)
            .Max();
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
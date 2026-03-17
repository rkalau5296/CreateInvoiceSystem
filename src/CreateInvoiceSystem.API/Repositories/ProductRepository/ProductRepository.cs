using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.API.Mappers.ProductMapper;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.API.Repositories.ProductRepository;

public class ProductRepository(IDbContext db) : IProductRepository
{
    private readonly IDbContext _db = db;

    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken)
    {
        var productEntity = ProductMapper.ToEntity(entity);
        await _db.Set<ProductEntity>().AddAsync(productEntity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return ProductMapper.ToDomain(productEntity);
    }

    public Task<bool> ExistsAsync(string name, int userId, CancellationToken cancellationToken)
    {
        return _db.Set<ProductEntity>()
           .AsNoTracking()
           .AnyAsync(p => p.Name == name && p.UserId == userId, cancellationToken);
    }

    public Task<bool> ExistsByIdAsync(int productId, CancellationToken cancellationToken) =>
        _db.Set<ProductEntity>()
           .AsNoTracking()
           .AnyAsync(p => p.ProductId == productId, cancellationToken);

    public async Task<PagedResult<Product>> GetAllAsync(int? userId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken)
    {
        var query = _db.Set<ProductEntity>().AsNoTracking();

        if (userId.HasValue)
            query = query.Where(p => p.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Name.Contains(searchTerm) || (p.Description != null && p.Description.Contains(searchTerm)));

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = ProductMapper.ToDomainList(products);

        return new PagedResult<Product>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Product> GetByIdAsync(int productId, int? userId, CancellationToken cancellationToken)
    {
        var query = _db.Set<ProductEntity>().AsNoTracking();

        if (userId.HasValue)
            query = query.Where(p => p.UserId == userId.Value);

        var productEntity = await query.SingleOrDefaultAsync(p => p.ProductId == productId, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {productId} not found.");

        return ProductMapper.ToDomain(productEntity);
    }

    public async Task RemoveAllByUserIdAsync(int userId, CancellationToken ct)
    {
        var products = await _db.Set<ProductEntity>()
            .Where(p => p.UserId == userId)
            .ToListAsync(ct);

        if (products.Count != 0)
            _db.Set<ProductEntity>().RemoveRange(products);
    }

    public async Task RemoveAsync(int productId, CancellationToken cancellationToken)
    {
        var productEntity = await _db.Set<ProductEntity>()
            .SingleOrDefaultAsync(c => c.ProductId == productId, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {productId} not found.");

        _db.Set<ProductEntity>().Remove(productEntity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);

    public async Task<Product> UpdateAsync(Product entity, CancellationToken cancellationToken)
    {
        var productEntity = ProductMapper.ToEntity(entity);

        _db.Set<ProductEntity>().Update(productEntity);
        await _db.SaveChangesAsync(cancellationToken);

        return ProductMapper.ToDomain(productEntity);
    }
}
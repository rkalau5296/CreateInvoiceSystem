using CreateInvoiceSystem.Abstractions.DbContext;
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
        var productEntity = new ProductEntity
        {
            Name = entity.Name,
            Description = entity.Description,
            Value = entity.Value,
            UserId = entity.UserId,
            IsDeleted = entity.IsDeleted
        };
        await _db.Set<ProductEntity>().AddAsync(productEntity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return new Product
        {
            ProductId = productEntity.ProductId,
            Name = productEntity.Name,
            Description = productEntity.Description,
            Value = productEntity.Value,
            UserId = productEntity.UserId,
            IsDeleted = productEntity.IsDeleted
        };
    }

    public Task<bool> ExistsAsync(string name, int userId, CancellationToken cancellationToken)
    {
         var exists = _db.Set<ProductEntity>()
            .AsNoTracking()
            .AnyAsync(p => p.Name == name && p.UserId == userId, cancellationToken);
        
         return exists;
    }

    public Task<bool> ExistsByIdAsync(int productId, CancellationToken cancellationToken) => _db.Set<ProductEntity>()
        .AsNoTracking()
        .AnyAsync(p => p.ProductId == productId, cancellationToken);

    public async Task<List<Product>> GetAllAsync(int? userId, CancellationToken cancellationToken)
    {
        var query = _db.Set<ProductEntity>()
            .AsNoTracking();
        
        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        var products = await query.ToListAsync(cancellationToken) ?? throw new InvalidOperationException($"No products found.");         

        return [.. products.Select(p => new Product
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Value = p.Value,
            UserId = p.UserId
        })];
    }

    public async Task<Product> GetByIdAsync(int productId, int? userId, CancellationToken cancellationToken)
    {
        var query = _db.Set<ProductEntity>().AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        var product = await query.SingleOrDefaultAsync(p => p.ProductId == productId, cancellationToken) ?? throw new InvalidOperationException($"Product with ID {productId} not found.");

        return new Product
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Description = product.Description,
            Value = product.Value,
            UserId = product.UserId
        };
    }

    public async Task RemoveAsync(int productId, CancellationToken cancellationToken)
    {
        var productEntity = await _db.Set<ProductEntity>()
            .SingleOrDefaultAsync(c => c.ProductId == productId, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {productId} not found.");

        _db.Set<ProductEntity>().Remove(productEntity);
        await _db.SaveChangesAsync(cancellationToken);        
    }        

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await _db.SaveChangesAsync(cancellationToken);

    public async Task<Product> UpdateAsync(Product entity, CancellationToken cancellationToken)
    {
        var productEntity = new ProductEntity
        {
            ProductId = entity.ProductId,
            Name = entity.Name,
            Description = entity.Description,
            Value = entity.Value,
            UserId = entity.UserId,
            IsDeleted = entity.IsDeleted
        };

        _db.Set<ProductEntity>().Update(productEntity);
        await _db.SaveChangesAsync(cancellationToken);

        return new Product
        {
            ProductId = productEntity.ProductId,
            Name = productEntity.Name,
            Description = productEntity.Description,
            Value = productEntity.Value,
            UserId = productEntity.UserId,
            IsDeleted = productEntity.IsDeleted
        };
    }
}
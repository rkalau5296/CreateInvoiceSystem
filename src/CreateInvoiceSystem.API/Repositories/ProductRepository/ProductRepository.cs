using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.API.Repositories.ProductRepository;

public class ProductRepository(IDbContext db) : IProductRepository
{
    private readonly IDbContext _db = db;

    public Task AddAsync(Product entity, CancellationToken cancellationToken) => _db.Set<Product>().AddAsync(entity, cancellationToken).AsTask();

    public Task<bool> ExistsAsync(string name, int userId, CancellationToken cancellationToken) =>
        _db.Set<Product>()
            .AsNoTracking()
            .AnyAsync(p => p.Name == name && p.UserId == userId, cancellationToken);

    public Task<bool> ExistsByIdAsync(int productId, CancellationToken cancellationToken) => _db.Set<Product>()
        .AsNoTracking()
        .AnyAsync(p => p.ProductId == productId, cancellationToken);

    public async Task<List<Product>> GetAllAsync(bool excludeDeleted, CancellationToken cancellationToken)
    {
        return await _db.Set<Product>()
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"List of addresses is empty.");
    }

    public Task<Product> GetByIdAsync(int productId, CancellationToken cancellationToken) => 
        _db.Set<Product>()
       .AsNoTracking()
       .SingleOrDefaultAsync(p => p.ProductId == productId, cancellationToken);    

    public async Task RemoveAsync(Product entity, CancellationToken cancellationToken)
    {
        _db.Set<Product>().Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }        

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await _db.SaveChangesAsync(cancellationToken);

    public async Task UpdateAsync(Product entity, CancellationToken cancellationToken)
    {
        _db.Set<Product>().Update(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
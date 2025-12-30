using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;

namespace CreateInvoiceSystem.Modules.Products.Domain.Interfaces;

public interface IProductRepository : ISaveChangesContext
{
    Task<bool> ExistsAsync(string name, int userId, CancellationToken cancellationToken);
    Task<Product> AddAsync(Product entity, CancellationToken cancellationToken);
    Task<Product> GetByIdAsync(int productId, CancellationToken cancellationToken);
    Task<Product> UpdateAsync(Product entity, CancellationToken cancellationToken);
    Task RemoveAsync(int productId, CancellationToken cancellationToken);
    Task<bool> ExistsByIdAsync(int productId, CancellationToken cancellationToken);    
    Task<List<Product>> GetAllAsync(CancellationToken cancellationToken);
}

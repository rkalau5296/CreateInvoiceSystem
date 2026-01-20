using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
public interface IInvoiceRepository : ISaveChangesContext
{
    Task<Invoice> AddInvoiceAsync(Invoice entity, CancellationToken cancellationToken);
    Task AddClientAsync(Client entity, CancellationToken cancellationToken);
    Task AddProductAsync(Product entity, CancellationToken cancellationToken);
    Task<Invoice> GetInvoiceByIdAsync(int? userId, int invoiceId, CancellationToken cancellationToken);
    Task<Client> GetClientByIdAsync(int? clientId, CancellationToken cancellationToken);
    Task<Product> GetProductByIdAsync(int productId, CancellationToken cancellationToken);
    Task<Client> GetClientAsync(string name, string street, string number, string city, string postalCode,
        string country, int userId, CancellationToken cancellationToken);
    Task<Product> GetProductAsync(string name, string description, decimal? value, int userId, CancellationToken cancellationToken);
    Task<PagedResult<Invoice>> GetInvoicesAsync(int? userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task AddInvoicePositionAsync(ICollection<InvoicePosition> invoicePositions, CancellationToken cancellationToken);
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken);
    Task RemoveRangeAsync(IEnumerable<InvoicePosition> invoicePositions, CancellationToken cancellationToken);
    Task RemoveAsync(Invoice invoiceEntity);
    Task RemoveInvoicePositionsAsync(InvoicePosition invoicePosition);
    Task<bool> InvoiceExistsAsync(int invoiceId, CancellationToken cancellationToken);
    Task<bool> InvoicePositionExistsAsync(int invoiceId, CancellationToken cancellationToken);
    Task<string> GetUserEmailByIdAsync(int userId, CancellationToken ct);
}

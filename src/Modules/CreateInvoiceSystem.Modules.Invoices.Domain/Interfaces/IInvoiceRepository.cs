using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
public interface IInvoiceRepository : ISaveChangesContext
{
    Task AddInvoiceAsync(Invoice entity, CancellationToken cancellationToken);
    Task AddClientAsync(Client entity, CancellationToken cancellationToken);
    Task AddProductAsync(Product entity, CancellationToken cancellationToken);
    Task<Invoice> GetInvoiceByIdAsync(int invoiceId, bool includeClient, bool includeClientAddress, bool includePositions, bool includeProducts, CancellationToken cancellationToken);
    Task<Client> GetClientByIdAsync(int clientId, bool includeAddress, CancellationToken cancellationToken);
    Task<Product> GetProductByIdAsync(int productId, CancellationToken cancellationToken);
    Task<Client> GetClientAsync(string name, string street, string number, string city, string postalCode,
        string country, int userId, CancellationToken cancellationToken);
    Task<Product> GetProductAsync(string name, string description, decimal? value, int userId, CancellationToken cancellationToken);
    Task<List<Invoice>> GetInvoicesAsync(CancellationToken cancellationToken);
    Task AddInvoicePositionAsync(InvoicePosition invoicePosition, CancellationToken cancellationToken);
    Task RemoveRangeAsync(IEnumerable<InvoicePosition> invoicePositions, CancellationToken cancellationToken);
    Task RemoveAsync(Invoice invoiceEntity);
    Task RemoveInvoicePositionsAsync(InvoicePosition invoicePosition);
    Task<bool> InvoiceExistsAsync(int invoiceId, CancellationToken cancellationToken);
    Task<bool> InvoicePositionExistsAsync(int invoiceId, CancellationToken cancellationToken);
}

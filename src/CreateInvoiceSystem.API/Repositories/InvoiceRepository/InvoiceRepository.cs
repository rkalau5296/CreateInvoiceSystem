using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.API.Repositories.InvoiceRepository
{
    public class InvoiceRepository(IDbContext db) : IInvoiceRepository
    {
        private readonly IDbContext _db = db;

        public Task AddClientAsync(Client entity, CancellationToken cancellationToken) =>
            _db.Set<Client>().AddAsync(entity, cancellationToken).AsTask();

        public Task AddInvoiceAsync(Invoice entity, CancellationToken cancellationToken) =>
            _db.Set<Invoice>().AddAsync(entity, cancellationToken).AsTask();

        public Task AddInvoicePositionAsync(InvoicePosition invoicePosition, CancellationToken cancellationToken) =>
            _db.Set<InvoicePosition>().AddAsync(invoicePosition, cancellationToken).AsTask();

        public Task AddProductAsync(Product entity, CancellationToken cancellationToken) =>
            _db.Set<Product>().AddAsync(entity, cancellationToken).AsTask();

        public async Task<Client> GetClientAsync(string name, string street, string number, string city, string postalCode, string country, int userId, CancellationToken cancellationToken)
        {
            var client = await _db.Set<Client>()
                .Include(c => c.Address)
                .FirstOrDefaultAsync(c =>
                    c.Name == name &&
                    c.UserId == userId &&
                    c.Address != null &&
                    c.Address.Street == street &&
                    c.Address.Number == number &&
                    c.Address.City == city &&
                    c.Address.PostalCode == postalCode &&
                    c.Address.Country == country,
                    cancellationToken);

            return client ?? throw new InvalidOperationException("Client not found with provided details.");
        }

        public async Task<Client> GetClientByIdAsync(int clientId, bool includeAddress, CancellationToken cancellationToken)
        {
            IQueryable<Client> query = _db.Set<Client>().AsNoTracking();
            if (includeAddress)
                query = query.Include(c => c.Address);

            return await query.SingleOrDefaultAsync(c => c.ClientId == clientId, cancellationToken) ?? throw new InvalidOperationException($"Client with ID {clientId} not found.");           
        }

        public async Task<Invoice> GetInvoiceByIdAsync(int invoiceId, bool includeClient, bool includeClientAddress, bool includePositions, bool includeProducts, CancellationToken cancellationToken)
        {
            IQueryable<Invoice> query = _db.Set<Invoice>().AsNoTracking();

            query = includeClientAddress
                     ? query.Include(i => i.Client).ThenInclude(c => c.Address)
                     : includeClient
                         ? query.Include(i => i.Client)
                         : query;

            query = includeProducts
                    ? query.Include(i => i.InvoicePositions).ThenInclude(p => p.Product)
                    : includePositions
                        ? query.Include(i => i.InvoicePositions)
                        : query;

            return await query.SingleOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken) ?? throw new InvalidOperationException($"Invoice with ID {invoiceId} not found.");
        }

        public Task<List<Invoice>> GetInvoicesAsync(CancellationToken cancellationToken) =>
            _db.Set<Invoice>()
               .AsNoTracking()
               .ToListAsync(cancellationToken);

        public async Task<Product> GetProductAsync(string name, string description, decimal? value, int userId, CancellationToken cancellationToken)
        {
            var query = _db.Set<Product>().AsQueryable();

            query = query.Where(p =>
                p.Name == name &&
                p.Description == description &&
                p.UserId == userId);

            if (value is not null)
                query = query.Where(p => p.Value == value);

            return await query.FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException("Product not found with provided details.");            
        }

        public async Task<Product> GetProductByIdAsync(int productId, CancellationToken cancellationToken) =>
            await _db.Set<Product>()
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.ProductId == productId, cancellationToken)
                ?? throw new InvalidOperationException($"Product with ID {productId} not found.");

        public Task<bool> InvoiceExistsAsync(int invoiceId, CancellationToken cancellationToken) =>
            _db.Set<Invoice>()
            .AsNoTracking()
            .AnyAsync(i => i.InvoiceId == invoiceId, cancellationToken);

        public Task<bool> InvoicePositionExistsAsync(int invoiceId, CancellationToken cancellationToken) =>
             _db.Set<InvoicePosition>()
            .AsNoTracking()
            .AnyAsync(ip => ip.InvoiceId == invoiceId, cancellationToken);

        public async Task RemoveAsync(Invoice invoiceEntity)
        {
            _db.Set<Invoice>().Remove(invoiceEntity);
            await _db.SaveChangesAsync(CancellationToken.None);
        }

        public async Task RemoveInvoicePositionsAsync(InvoicePosition invoicePosition)
        {
            _db.Set<InvoicePosition>().Remove(invoicePosition);
            await _db.SaveChangesAsync(CancellationToken.None);
        }

        public async Task RemoveRangeAsync(IEnumerable<InvoicePosition> invoicePositions, CancellationToken cancellationToken)
        {
            _db.Set<InvoicePosition>().RemoveRange(invoicePositions);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _db.SaveChangesAsync(cancellationToken);
    }
}

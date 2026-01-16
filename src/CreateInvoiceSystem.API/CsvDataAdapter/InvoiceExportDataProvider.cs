using CreateInvoiceSystem.Csv.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;

namespace CreateInvoiceSystem.API.CsvDataAdapter
{
    public class InvoiceExportDataProvider(IInvoiceRepository _invoiceRepository, IProductRepository _productRepository, IClientRepository _clientRepository) : IExportDataProvider
    {
        public async Task<IEnumerable<object>> GetInvoicesDataAsync(int userId)
        {
            var invoices = await _invoiceRepository.GetInvoicesAsync(userId, CancellationToken.None);

            return invoices.Select(i => new
            {
                Numer = i.Title, 
                Kontrahent = i.ClientName,
                Nip = i.ClientNip,
                DataPlatnosci = i.PaymentDate.ToShortDateString(),
                Kwota = i.TotalAmount
            });
        }
        public async Task<IEnumerable<object>> GetProductsDataAsync(int userId)
        {
            var products = await _productRepository.GetAllAsync(userId, CancellationToken.None);
            return products.Select(p => new
            {
                Nazwa = p.Name,
                Cena = p.Value,
                Uzytkownik = p.UserId 
            });
        }

        public async Task<IEnumerable<object>> GetClientsDataAsync(int userId)
        {
            var clients = await _clientRepository.GetAllAsync(userId, CancellationToken.None);
            return clients.Select(c => new
            {
                NazwaFirmy = c.Name,
                NIP = c.Nip,
                Miasto = c.Address?.City,
                Ulica = c.Address?.Street
            });
        }
    }
}

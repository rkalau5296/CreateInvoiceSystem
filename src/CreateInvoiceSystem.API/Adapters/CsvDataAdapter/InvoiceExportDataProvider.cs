using CreateInvoiceSystem.Csv.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;

namespace CreateInvoiceSystem.API.Adapters.CsvDataAdapter
{
    public class InvoiceExportDataProvider(IInvoiceRepository _invoiceRepository, IProductRepository _productRepository, IClientRepository _clientRepository) : IExportDataProvider
    {
        public async Task<IEnumerable<object>> GetInvoicesDataAsync(int userId)
        {
            var pagedResult = await _invoiceRepository.GetInvoicesAsync(userId, 1, int.MaxValue, null, CancellationToken.None);
            var invoices = pagedResult.Items;

            return invoices.Select(i => new
            {
                Numer = i.Title,
                Kontrahent = i.ClientName,
                Nip = i.ClientNip,
                DataPlatnosci = i.PaymentDate.ToShortDateString(),
                Kwota = i.TotalGross
            });
        }
        public async Task<IEnumerable<object>> GetProductsDataAsync(int userId)
        {            
            var pagedResult = await _productRepository.GetAllAsync(userId, 1, int.MaxValue, null, CancellationToken.None);

            return pagedResult.Items.Select(p => new
            {
                Nazwa = p.Name,
                Cena = p.Value,
                Uzytkownik = p.UserId
            });
        }

        public async Task<IEnumerable<object>> GetClientsDataAsync(int userId)
        {            
            var pagedResult = await _clientRepository.GetAllAsync(userId, 1, int.MaxValue, null, CancellationToken.None);
            
            return pagedResult.Items.Select(c => new
            {
                NazwaFirmy = c.Name,
                NIP = c.Nip,
                Miasto = c.Address?.City,
                Ulica = c.Address?.Street
            });
        }
    }
}

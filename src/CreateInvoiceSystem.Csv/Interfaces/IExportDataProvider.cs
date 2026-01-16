namespace CreateInvoiceSystem.Csv.Interfaces;

public interface IExportDataProvider
{
    Task<IEnumerable<object>> GetInvoicesDataAsync(int userId);
    Task<IEnumerable<object>> GetProductsDataAsync(int userId); 
    Task<IEnumerable<object>> GetClientsDataAsync(int userId);
}

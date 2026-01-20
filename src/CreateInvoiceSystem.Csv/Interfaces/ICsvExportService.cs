namespace CreateInvoiceSystem.Csv.Interfaces;

public interface ICsvExportService
{
    byte[] ExportToCsv(IEnumerable<object> data);    
}
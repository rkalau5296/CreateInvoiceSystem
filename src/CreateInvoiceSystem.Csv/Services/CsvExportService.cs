using CreateInvoiceSystem.Csv.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

namespace CreateInvoiceSystem.Csv.Services;

public class CsvExportService : ICsvExportService
{
    public byte[] ExportToCsv(IEnumerable<object> data) 
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {            
            csv.WriteRecords(data);
            writer.Flush();
        }
        return memoryStream.ToArray();
    }
}

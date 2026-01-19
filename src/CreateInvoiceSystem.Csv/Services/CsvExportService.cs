using CreateInvoiceSystem.Csv.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

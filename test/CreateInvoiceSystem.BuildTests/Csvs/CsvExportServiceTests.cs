using CreateInvoiceSystem.Csv.Services;
using FluentAssertions;
using System.Text;

namespace CreateInvoiceSystem.BuildTests.Csvs;

public class CsvExportServiceTests
{
    private readonly CsvExportService _csvExportService;

    public CsvExportServiceTests()
    {
        _csvExportService = new CsvExportService();
    }

    [Fact]
    public void ExportToCsv_ShouldReturnValidCsvBytes_WhenDataIsProvided()
    {
        // Arrange
        var data = new List<TestData>
        {
            new() { Id = 1, Name = "Test Product", Price = 10.50m },
            new() { Id = 2, Name = "Second Item", Price = 20.00m }
        };

        // Act
        var result = _csvExportService.ExportToCsv(data);
        var csvString = Encoding.UTF8.GetString(result);

        // Assert
        result.Should().NotBeEmpty();
        
        csvString.Should().Contain("Id,Name,Price");
        
        csvString.Should().Contain("1,Test Product,10.50");
        csvString.Should().Contain("2,Second Item,20.00");
    }

    [Fact]
    public void ExportToCsv_ShouldReturnEmpty_WhenDataIsEmptyAndTypeIsObject()
    {
        // Arrange
        var data = new List<object>(); 

        // Act
        var result = _csvExportService.ExportToCsv(data);
        var cleanCsvString = Encoding.UTF8.GetString(result).Replace("\uFEFF", "").Trim();

        // Assert
        cleanCsvString.Should().BeEmpty();
    }

    [Fact]
    public void ExportToCsv_ShouldReturnData_WhenListIsNotEmpty()
    {
        // Arrange
        var data = new List<TestData>
        {
            new() { Id = 1, Name = "Faktura 1", Price = 100.00m }
        };

        // Act
        var result = _csvExportService.ExportToCsv(data);
        var cleanCsvString = Encoding.UTF8.GetString(result).Replace("\uFEFF", "").Trim();
        var lines = cleanCsvString.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        // Assert
        lines[0].Should().Be("Id,Name,Price");
        lines[1].Should().Be("1,Faktura 1,100.00");
    }

    [Fact]
    public void ExportToCsv_ShouldReturnHeadersAndData_WhenDataExists()
    {
        // Arrange       
        var data = new List<TestData>
        {
            new() { Id = 1, Name = "Produkt", Price = 99.99m }
        };

        // Act
        var result = _csvExportService.ExportToCsv(data);
        var cleanCsvString = Encoding.UTF8.GetString(result).Replace("\uFEFF", "").Trim();
        var lines = cleanCsvString.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        // Assert
        lines[0].Should().Be("Id,Name,Price");
        lines[1].Should().Be("1,Produkt,99.99");
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
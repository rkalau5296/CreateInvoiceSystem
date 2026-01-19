using CreateInvoiceSystem.Pdf; 
using CreateInvoiceSystem.Pdf.Models;
using FluentAssertions;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Pdf;

public class QuestPdfGeneratorTests
{
    public QuestPdfGeneratorTests()
    {        
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    [Fact]
    public void Create_ShouldReturnNonEmptyByteArray_WhenRequestIsValid()
    {
        // Arrange
        var generator = new QuestPdfGenerator();
        
        var rows = new List<PdfRow>
        {
            new PdfRow(
                Name: "Produkt testowy",
                Quantity: 1,
                UnitPrice: 100m,
                NetValue: 100m,
                VatRate: 23,
                VatValue: 23m,
                TotalPrice: 123m)
        };

        var sections = new List<PdfTableSection>
        {
            new PdfTableSection(rows)
        };
        
        var request = new PdfDocumentRequest(
            Title: "Test PDF",
            Subtitle: "Podtytuł testowy",
            ClientName: "Klient Testowy",
            ClientAddress: "Adres 1",
            ClientNip: "123",
            UserName: "User Test",
            UserAddress: "Adres 2",
            UserNip: "456",
            Sections: sections,
            FooterText: "Stopka testowa",
            PaymentMethod: "Przelew",
            PaymentDueDate: DateTime.Now.AddDays(7),
            BankAccountNumber: "12 3456 7890 1234 5678 9012 3456"
        );

        // Act
        var result = generator.Create(request);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);        
    }
}
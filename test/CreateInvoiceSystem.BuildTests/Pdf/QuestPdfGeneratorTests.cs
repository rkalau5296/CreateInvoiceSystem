using CreateInvoiceSystem.Modules.Pdf.Infrastructure;
using CreateInvoiceSystem.Pdf.Models;
using FluentAssertions;

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
            new PdfRow("Produkt testowy", 1, 100m, 100m)
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
            FooterText: "Stopka testowa"
        );

        // Act
        var result = generator.Create(request);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }
}
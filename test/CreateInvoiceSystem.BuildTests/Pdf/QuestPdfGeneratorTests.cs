using CreateInvoiceSystem.Modules.Pdf.Infrastructure;
using CreateInvoiceSystem.Pdf.Models;
using FluentAssertions;

namespace CreateInvoiceSystem.BuildTests.Pdf;

public class QuestPdfGeneratorTests
{
    [Fact]
    public void Create_ShouldReturnNonEmptyByteArray_WhenRequestIsValid()
    {
        // Arrange
        var generator = new QuestPdfGenerator();
        var request = new PdfDocumentRequest(
            Title: "Test PDF",
            Subtitle: "Podtytuł testowy",
            Sections: new List<PdfTableSection>
            {
                new PdfTableSection(
                    Name: "Sekcja testowa",
                    Headers: new[] { "Nagłówek 1", "Nagłówek 2" },
                    Rows: new List<string[]> { new[] { "Komórka 1", "Komórka 2" } }
                )
            },
            FooterText: "Stopka"
        );

        // Act
        var result = generator.Create(request);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }
}
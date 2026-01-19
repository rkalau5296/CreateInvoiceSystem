using CreateInvoiceSystem.Pdf.Interfaces;
using CreateInvoiceSystem.Pdf.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CreateInvoiceSystem.Modules.Pdf.Infrastructure;

public class QuestPdfGenerator : IPdfGenerator
{
    public byte[] Create(PdfDocumentRequest request)
    {        
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Verdana));
                
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(request.Title).FontSize(22).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text(request.Subtitle).FontSize(14).Italic();
                    });
                });
                
                page.Content().PaddingVertical(20).Column(col =>
                {
                    foreach (var section in request.Sections)
                    {
                        col.Item().PaddingTop(10).PaddingBottom(5).Text(section.Name).SemiBold();

                        col.Item().Table(table =>
                        {                            
                            table.ColumnsDefinition(columns =>
                            {
                                for (int i = 0; i < section.Headers.Length; i++)
                                    columns.RelativeColumn();
                            });
                            
                            table.Header(header =>
                            {
                                foreach (var h in section.Headers)
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(h).SemiBold();
                            });
                            
                            foreach (var rowData in section.Rows)
                            {
                                foreach (var cellValue in rowData)
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5).Text(cellValue);
                            }
                        });
                    }
                });
                
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span($"{request.FooterText} | Strona ");
                    x.CurrentPageNumber();
                });
            });
        }).GeneratePdf();
    }
}
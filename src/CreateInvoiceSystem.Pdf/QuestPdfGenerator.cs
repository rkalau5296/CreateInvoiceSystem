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
        static IContainer HeaderStyle(IContainer c) =>
            c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);

        static IContainer RowStyle(IContainer c) =>
            c.PaddingVertical(5).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));
               
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text(request.Title).FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                        row.RelativeItem().AlignRight().Text(request.Subtitle).FontSize(10).Italic();
                    });

                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                    col.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Sprzedawca:").Bold();
                            c.Item().Text(request.UserName);
                            c.Item().Text(request.UserAddress);
                            c.Item().Text($"NIP: {request.UserNip}");
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().AlignRight().Text("Nabywca:").Bold();
                            c.Item().AlignRight().Text(request.ClientName);
                            c.Item().AlignRight().Text(request.ClientAddress);
                            c.Item().AlignRight().Text($"NIP: {request.ClientNip}");
                        });
                    });
                });
                
                page.Content().PaddingVertical(20).Column(col =>
                {
                    foreach (var section in request.Sections)
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Lp.");
                                header.Cell().Element(HeaderStyle).Text("Nazwa");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Ilość");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Cena");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Suma");
                            });

                            int index = 1;
                            foreach (var row in section.Rows)
                            {
                                table.Cell().Element(RowStyle).Text(index.ToString());
                                table.Cell().Element(RowStyle).Text(row.Name ?? string.Empty);
                                table.Cell().Element(RowStyle).AlignRight().Text(row.Quantity.ToString());
                                table.Cell().Element(RowStyle).AlignRight().Text(row.UnitPrice.ToString("N2"));
                                table.Cell().Element(RowStyle).AlignRight().Text(row.TotalPrice.ToString("N2"));

                                index++;
                            }
                        });
                    }
                });
                
                page.Footer().Column(col =>
                {
                    col.Item().PaddingTop(10).LineHorizontal(0.5f);
                    col.Item().PaddingTop(5).Text(request.FooterText).FontSize(9).Italic();
                    col.Item().AlignRight().Text(x =>
                    {
                        x.Span("Strona ");
                        x.CurrentPageNumber();
                    });
                });
            });
        }).GeneratePdf();
    }
}
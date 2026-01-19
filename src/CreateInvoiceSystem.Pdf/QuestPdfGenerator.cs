using CreateInvoiceSystem.Pdf.Interfaces;
using CreateInvoiceSystem.Pdf.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;

namespace CreateInvoiceSystem.Pdf;

public class QuestPdfGenerator : IPdfGenerator
{
    public byte[] Create(PdfDocumentRequest request)
    {
        static IContainer HeaderStyle(IContainer c) =>
            c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);

        static IContainer RowStyle(IContainer c) =>
            c.PaddingVertical(5).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);

        var allRows = request.Sections.SelectMany(x => x.Rows).ToList();
        var totalGross = allRows.Sum(x => x.TotalPrice);

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
                                columns.ConstantColumn(25);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Lp.");
                                header.Cell().Element(HeaderStyle).Text("Nazwa");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Ilość");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Cena netto");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Wartość netto");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("VAT");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Wartość VAT");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Wartość brutto");
                            });

                            int index = 1;
                            foreach (var row in section.Rows)
                            {
                                table.Cell().Element(RowStyle).Text(index.ToString());
                                table.Cell().Element(RowStyle).Text(row.Name ?? string.Empty);
                                table.Cell().Element(RowStyle).AlignRight().Text(row.Quantity.ToString());
                                table.Cell().Element(RowStyle).AlignRight().Text(row.UnitPrice.ToString("N2"));
                                table.Cell().Element(RowStyle).AlignRight().Text(row.NetValue.ToString("N2"));
                                table.Cell().Element(RowStyle).AlignRight().Text($"{row.VatRate}%");
                                table.Cell().Element(RowStyle).AlignRight().Text(row.VatValue.ToString("N2"));
                                table.Cell().Element(RowStyle).AlignRight().Text(row.TotalPrice.ToString("N2"));

                                index++;
                            }
                        });
                    }

                    
                    col.Item().PaddingTop(20).Row(row =>
                    {
                    
                        row.RelativeItem().Column(payCol =>
                        {
                            payCol.Item().Text("Dane płatności:").SemiBold().Underline();
                            payCol.Item().Text($"Forma płatności: {request.PaymentMethod}");
                            payCol.Item().Text($"Termin płatności: {request.PaymentDueDate:yyyy-MM-dd}");
                            payCol.Item().PaddingTop(5).Text("Numer konta:");
                            payCol.Item().Text(request.BankAccountNumber).Bold();
                        });
                        
                        row.RelativeItem().Column(totalCol =>
                        {
                            totalCol.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderStyle).Text("Stawka");
                                    header.Cell().Element(HeaderStyle).AlignRight().Text("Netto");
                                    header.Cell().Element(HeaderStyle).AlignRight().Text("VAT");
                                    header.Cell().Element(HeaderStyle).AlignRight().Text("Brutto");
                                });

                                var vatGroups = allRows.GroupBy(x => x.VatRate);
                                foreach (var group in vatGroups)
                                {
                                    table.Cell().Element(RowStyle).Text($"{group.Key}%");
                                    table.Cell().Element(RowStyle).AlignRight().Text(group.Sum(x => x.NetValue).ToString("N2"));
                                    table.Cell().Element(RowStyle).AlignRight().Text(group.Sum(x => x.VatValue).ToString("N2"));
                                    table.Cell().Element(RowStyle).AlignRight().Text(group.Sum(x => x.TotalPrice).ToString("N2"));
                                }
                            });

                            totalCol.Item().PaddingTop(10).AlignRight().Text(x =>
                            {
                                x.Span("RAZEM: ").FontSize(12).SemiBold();
                                x.Span($"{totalGross:N2} zł").FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                            });
                        });
                    });

                    
                    col.Item().PaddingTop(50).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().PaddingHorizontal(20).LineHorizontal(0.5f);
                            c.Item().AlignCenter().Text("Osoba upoważniona do odbioru").FontSize(8);
                        });

                        row.ConstantItem(50); 

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().PaddingHorizontal(20).LineHorizontal(0.5f);
                            c.Item().AlignCenter().Text("Osoba upoważniona do wystawienia").FontSize(8);
                        });
                    });
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
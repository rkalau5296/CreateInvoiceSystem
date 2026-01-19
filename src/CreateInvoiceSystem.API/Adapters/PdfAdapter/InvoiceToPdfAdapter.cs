using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Pdf.Interfaces;
using CreateInvoiceSystem.Pdf.Models;

namespace CreateInvoiceSystem.API.Adapters.PdfAdapter
{
    public class InvoiceToPdfAdapter(IPdfGenerator _pdfGenerator, IUserRepository _userRepository) : IInvoiceExportService
    {
        public async Task<byte[]> ExportToPdfAsync(InvoiceDto invoice, int userId, CancellationToken cancellationToken)
        {            
            var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken) ?? throw new Exception($"Błąd: Nie znaleziono danych sprzedawcy o ID: {userId}");            

            string formattedUserAddress = user.Address != null
                ? $"{user.Address.Street}, {user.Address.PostalCode} {user.Address.City}"
                : "Brak adresu";

            var rows = invoice.InvoicePositions.Select(pos => new PdfRow(
                Name: pos.ProductName ?? "Brak nazwy",
                Quantity: pos.Quantity,
                UnitPrice: pos.ProductValue ?? 0,
                TotalPrice: (pos.ProductValue ?? 0) * pos.Quantity
            )).ToList();

            var pdfRequest = new PdfDocumentRequest(
                Title: $"Faktura nr {invoice.Title}",
                Subtitle: $"Data wystawienia: {DateTime.Now:dd.MM.yyyy}",
                ClientName: invoice.ClientName ?? "Brak danych",
                ClientAddress: invoice.ClientAddress ?? "Brak adresu",
                ClientNip: invoice.ClientNip ?? "-",
                UserName: user.CompanyName ?? user.Name,
                UserAddress: formattedUserAddress,
                UserNip: user.Nip ?? "-",
                Sections: new List<PdfTableSection> { new PdfTableSection(rows) },
                FooterText: "Dziękujemy za współpracę!"
            );

            return _pdfGenerator.Create(pdfRequest);
        }
    }
}

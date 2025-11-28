namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class InvoiceMappers
{
    public static InvoiceDto ToDto(this Invoice invoice) =>
        new(invoice.InvoiceId, invoice.Title, invoice.TotalAmount, invoice.PaymentDate, invoice.CreatedDate, invoice.Comments, invoice.ClientId, invoice.UserId, invoice.Client.ToDto(), invoice.User.ToDto(), invoice.MethodOfPayment, invoice.InvoicePositions);

    public static Invoice ToEntity(this InvoiceDto dto) =>
        new()
        {
            InvoiceId = dto.InvoiceId,
            Title = dto.Title,
            TotalAmount = dto.TotalAmount,
            PaymentDate = dto.PaymentDate,
            CreatedDate = dto.CreatedDate,
            Comments = dto.Comments,
            ClientId = dto.ClientId,
            UserId = dto.UserId,            
            Client = dto.Client.ToEntity(),
            User = dto.User.ToEntity(),
            MethodOfPayment = dto.MethodOfPayment,
            InvoicePositions = dto.InvoicePositions
        };

    public static List<InvoiceDto> ToDtoList(this IEnumerable<Invoice> Invoices) =>
         [.. Invoices.Select(a => a.ToDto())];
}

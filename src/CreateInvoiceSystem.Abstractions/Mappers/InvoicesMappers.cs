namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class InvoiceMappers
{
    public static InvoiceDto ToDto(this Invoice Invoice) =>
        new(Invoice.InvoiceId, Invoice.Title, Invoice.Value, Invoice.PaymentDate, Invoice.CreatedDate, Invoice.Comments, Invoice.ClientId, Invoice.UserId, Invoice.Client.ToDto(), Invoice.User.ToDto(), Invoice.MethodOfPayment, [.. Invoice.Products.Select(p=>p.ToDto())], Invoice.Product);

    public static Invoice ToEntity(this InvoiceDto dto) =>
        new()
        {
            InvoiceId = dto.InvoiceId,
            Title = dto.Title,
            Value = dto.Value,
            PaymentDate = dto.PaymentDate,
            CreatedDate = dto.CreatedDate,
            Comments = dto.Comments,
            ClientId = dto.ClientId,
            UserId = dto.UserId,            
            Client = dto.Client.ToEntity(),
            User = dto.User.ToEntity(),
            MethodOfPayment = dto.MethodOfPayment,
            Products = [.. dto.Products.Select(p => p.ToEntity())],
            Product = dto.Product
        };

    public static List<InvoiceDto> ToDtoList(this IEnumerable<Invoice> Invoices) =>
         [.. Invoices.Select(a => a.ToDto())];
}

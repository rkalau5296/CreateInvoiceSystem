namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Threading;

public static class InvoiceMappers
{
    public static InvoiceDto ToDto(this Invoice invoice) =>
        invoice == null
        ? throw new ArgumentNullException(nameof(invoice), "Invoice cannot be null when mapping to InvoiceDto.")
        :
        new(invoice.InvoiceId, invoice.Title, invoice.TotalAmount, invoice.PaymentDate, invoice.CreatedDate, invoice.Comments, invoice.ClientId, invoice.UserId, invoice.MethodOfPayment, invoice.InvoicePositions.Select(ip => ip.ToDto()).ToList());

    public static Invoice ToEntity(this InvoiceDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "InvoiceDto cannot be null when mapping to Invoice.")
        :
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
            MethodOfPayment = dto.MethodOfPayment,
            InvoicePositions = dto.InvoicePositionsDto.Select(ipDto => ipDto.ToEntity()).ToList()
        };

    public static Invoice ToInvoiceIfClientIdIsNull(this CreateInvoiceDto dto, Client client) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "InvoiceDto cannot be null when mapping to Invoice.")
        : client == null
        ? throw new ArgumentNullException(nameof(client), "Client cannot be null when mapping to Invoice.")
        : 
        new ()
        {
            Title = dto.Title,
            TotalAmount = dto.TotalAmount,
            PaymentDate = dto.PaymentDate,
            CreatedDate = dto.CreatedDate,
            Comments = dto.Comments,            
            UserId = dto.UserId,
            Client = client,
            MethodOfPayment = dto.MethodOfPayment,            
        };
    

    public static Invoice ToInvoiceIfClientIdIsNotNull(this CreateInvoiceDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "InvoiceDto cannot be null when mapping to Invoice.")
        :
        new()
        {
            Title = dto.Title,
            TotalAmount = dto.TotalAmount,
            PaymentDate = dto.PaymentDate,
            CreatedDate = dto.CreatedDate,
            Comments = dto.Comments,
            ClientId = dto.ClientId,
            UserId = dto.UserId,
            MethodOfPayment = dto.MethodOfPayment
        };  

    public static List<InvoiceDto> ToDtoList(this IEnumerable<Invoice> Invoices) =>
         Invoices == null
        ? throw new ArgumentNullException(nameof(Invoices), "Invoices cannot be null when mapping to InvoicesDto.")
        :
         [.. Invoices.Select(a => a.ToDto())];
}

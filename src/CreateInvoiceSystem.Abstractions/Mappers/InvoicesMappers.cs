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
            InvoicePositions = dto.InvoicePositions.Select(ipDto => ipDto.ToEntity()).ToList()
        };

    public static Invoice ToInvoiceWithNewClient(this CreateInvoiceDto dto, Client client)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "CreateInvoiceDto cannot be null when mapping to Invoice.");
        if (client == null)
            throw new ArgumentNullException(nameof(client), "Client cannot be null when mapping to Invoice.");

        return new Invoice
        {
            Title = dto.Title,
            TotalAmount = dto.TotalAmount,
            PaymentDate = dto.PaymentDate,
            CreatedDate = dto.CreatedDate,
            Comments = dto.Comments,
            UserId = dto.UserId,
            ClientId = null, 
            Client = client,
            MethodOfPayment = dto.MethodOfPayment,            
            ClientName = dto.Client.Name,
            ClientNip = dto.Client.Nip,
            ClientAddress = FormatAddress(dto.Client.Address),            
            InvoicePositions = []
        };
    }
    
    public static string FormatAddress(AddressDto address) =>
        address == null
            ? null
            : $"{address.Street} {address.Number}, {address.City}, {address.PostalCode}, {address.Country}";


    public static Invoice ToInvoiceWithExistingClient(this CreateInvoiceDto dto, Client client)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "CreateInvoiceDto cannot be null when mapping to Invoice.");
        if (client == null)
            throw new ArgumentNullException(nameof(client), "Client cannot be null when mapping to Invoice.");

        return new Invoice
        {
            Title = dto.Title,
            TotalAmount = dto.TotalAmount,
            PaymentDate = dto.PaymentDate,
            CreatedDate = dto.CreatedDate,
            Comments = dto.Comments,
            UserId = dto.UserId,
            ClientId = client.ClientId,
            Client = client,
            MethodOfPayment = dto.MethodOfPayment,            
            ClientName = client.Name,
            ClientNip = client.Nip,
            ClientAddress = FormatAddress(client.Address),
            InvoicePositions = []
        };
    }
    
    public static string FormatAddress(Address address) =>
        address == null
            ? null
            : $"{address.Street} {address.Number}, {address.City}, {address.PostalCode}, {address.Country}";

    public static List<InvoiceDto> ToDtoList(this IEnumerable<Invoice> Invoices) =>
         Invoices == null
        ? throw new ArgumentNullException(nameof(Invoices), "Invoices cannot be null when mapping to InvoicesDto.")
        :
         [.. Invoices.Select(a => a.ToDto())];


}

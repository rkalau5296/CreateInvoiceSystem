using CreateInvoiceSystem.Modules.Addresses.Dto;
using CreateInvoiceSystem.Modules.Addresses.Entities;
using CreateInvoiceSystem.Modules.Clients.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Dto;
using CreateInvoiceSystem.Modules.InvoicePositions.Mappers;
using CreateInvoiceSystem.Modules.Invoices.Dto;
using CreateInvoiceSystem.Modules.Invoices.Entities;

namespace CreateInvoiceSystem.Modules.Invoices.Mappers;

public static class InvoiceMappers
{
    public static InvoiceDto ToDto(this Invoice invoice) =>
        invoice == null
        ? throw new ArgumentNullException(nameof(invoice), "Invoice cannot be null when mapping to InvoiceDto.")
        :
        new(
            invoice.InvoiceId, 
            invoice.Title, 
            invoice.TotalAmount, 
            invoice.PaymentDate, 
            invoice.CreatedDate, 
            invoice.Comments, 
            invoice.ClientId,
            invoice.UserId, 
            invoice.MethodOfPayment, 
            invoice.InvoicePositions.Select(ip => ip.ToDto()).ToList(),
            invoice.ClientName,
            invoice.ClientNip,
            invoice.ClientAddress
            );

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

    public static UpdateInvoiceDto ToUpdateDto(this Invoice invoice) =>
    invoice is null
        ? throw new ArgumentNullException(nameof(invoice), "Invoice cannot be null when mapping to UpdateInvoiceDto.")
        : new UpdateInvoiceDto(
            invoice.InvoiceId,
            invoice.Title,
            invoice.TotalAmount,
            invoice.PaymentDate,
            invoice.CreatedDate,
            invoice.Comments,
            invoice.ClientId,
            invoice.UserId,
            invoice.MethodOfPayment,
            invoice.InvoicePositions?
                .Select(ip => new UpdateInvoicePositionDto(
                    ip.InvoicePositionId,
                    ip.InvoiceId,
                    ip.ProductName,
                    ip.ProductDescription,
                    ip.ProductValue,
                    ip.Quantity
                ))
                .ToList() ?? [],
            invoice.ClientName,
            invoice.ClientNip,
            invoice.ClientAddress
        );
}

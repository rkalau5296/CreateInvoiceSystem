using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;
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

    public static InvoicePositionDto ToDto(this InvoicePosition invoicePosition) =>
    invoicePosition == null
        ? throw new ArgumentNullException(nameof(invoicePosition))
        : new(
            invoicePosition.InvoicePositionId,
            invoicePosition.InvoiceId,
            invoicePosition.ProductId,
            invoicePosition.Product != null
                ? new ProductDto(
                    invoicePosition.Product.ProductId,
                    invoicePosition.Product.Name,
                    invoicePosition.Product.Description,
                    invoicePosition.Product.Value,
                    invoicePosition.Product.UserId,
                    invoicePosition.Product.IsDeleted
                  )
                : null,
            invoicePosition.ProductName,
            invoicePosition.ProductDescription,
            invoicePosition.ProductValue,
            invoicePosition.Quantity
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

    public static InvoicePosition ToEntity(this InvoicePositionDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "InvoicePositionDto cannot be null when mapping to InvoicePosition.")
        :
        new()
        {
            InvoicePositionId = dto.InvoicePositionId,
            InvoiceId = dto.InvoiceId,
            ProductId = dto.ProductId,
            Product = dto.Product.ToEntity(),
            Quantity = dto.Quantity
        };

    public static Product ToEntity(this ProductDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "Product cannot be null when mapping to Product.")
        :
        new()
        {
            ProductId = dto.ProductId,
            Name = dto.Name,
            Description = dto.Description,
            Value = dto.Value,
            UserId = dto.UserId
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
            ClientId = client.ClientId, 
            Client = client,
            MethodOfPayment = dto.MethodOfPayment,
            ClientName = client.Name,
            ClientNip = client.Nip,
            ClientAddress = FormatAddress(client.Address),
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
        ? throw new ArgumentNullException(nameof(invoice), "Invoice cannot be null.")
        : new UpdateInvoiceDto(
            invoice.InvoiceId,
            invoice.Title,
            invoice.TotalAmount,
            invoice.PaymentDate,
            invoice.CreatedDate,
            invoice.Comments,
            invoice.ClientId,
            invoice.UserId,
            invoice.Client != null ? new UpdateClientDto(
                invoice.Client.ClientId,
                invoice.Client.Name,
                invoice.Client.Nip,
                invoice.Client.Address != null ? new AddressDto(
                    invoice.Client.Address.AddressId,
                    invoice.Client.Address.Street,
                    invoice.Client.Address.Number,
                    invoice.Client.Address.City,
                    invoice.Client.Address.PostalCode,
                    invoice.Client.Address.Country
                ) : null,
                invoice.Client.UserId,
                false
            ) : null,
            invoice.MethodOfPayment,
            invoice.InvoicePositions?
                .Select(ip => new UpdateInvoicePositionDto(
                    ip.InvoicePositionId,
                    ip.InvoiceId,
                    ip.ProductId, 
                    ip.ProductName,
                    ip.ProductDescription,
                    ip.ProductValue,
                    ip.Quantity,
                    null
                )).ToList() ?? [],
            invoice.ClientName,
            invoice.ClientNip,
            invoice.ClientAddress
        );

    public static Client ToEntity(this CreateClientDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "Client cannot be null when mapping to Client.")
        :
        new()
        {
            Name = dto.Name,
            Nip = dto.Nip,
            Address = dto.Address.ToEntity(),
            UserId = dto.UserId,
            IsDeleted = dto.IsDeleted
        };

    public static Address ToEntity(this AddressDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "Address cannot be null when mapping to Address.")
        :
        new()
        {
            AddressId = dto.AddressId,
            Street = dto.Street,
            Number = dto.Number,
            City = dto.City,
            PostalCode = dto.PostalCode,
            Country = dto.Country
        };

    public static Address ToEntity(CreateAddressDto dto) =>
        dto == null
       ? throw new ArgumentNullException(nameof(dto), "Address cannot be null when mapping to Address.")
       :
        new Address
        {
            Street = dto.Street,
            Number = dto.Number,
            City = dto.City,
            PostalCode = dto.PostalCode,
            Country = dto.Country
        };
}

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
        invoice.TotalNet,
        invoice.TotalVat,
        invoice.TotalGross,
        invoice.PaymentDate,
        invoice.CreatedDate,
        invoice.Comments,
        null,
        invoice.UserId,
        invoice.MethodOfPayment,
        invoice.InvoicePositions.Select(ip => ip.ToDto()).ToList(),
        invoice.SellerName,     
        invoice.SellerNip,
        invoice.SellerAddress,        
        invoice.BankAccountNumber,
        invoice.ClientName,
        invoice.ClientNip,
        invoice.ClientAddress,
        invoice.ClientEmail,
        invoice.Currency
    );

    public static InvoicePositionDto ToDto(this InvoicePosition invoicePosition) =>
    invoicePosition == null
        ? throw new ArgumentNullException(nameof(invoicePosition))
        : new(
            invoicePosition.InvoicePositionId,
            invoicePosition.InvoiceId,
            null, 
            null!,
            invoicePosition.ProductName,
            invoicePosition.ProductDescription,
            invoicePosition.ProductValue,
            invoicePosition.Quantity,
            invoicePosition.VatRate,
            invoicePosition.UnitOfMeasure
        );

    public static Invoice ToInvoiceWithNewClient(this CreateInvoiceDto dto, Client client, User user)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "CreateInvoiceDto cannot be null when mapping to Invoice.");
        if (client == null)
            throw new ArgumentNullException(nameof(client), "Client cannot be null when mapping to Invoice.");
        if (user == null)
            throw new ArgumentNullException(nameof(user), "User cannot be null when mapping to Invoice.");

        return new Invoice
        {
            Title = dto.Title,
            TotalNet = dto.TotalNet,
            TotalVat = dto.TotalVat,
            TotalGross = dto.TotalGross,
            PaymentDate = dto.PaymentDate,
            CreatedDate = dto.CreatedDate,
            Comments = dto.Comments,
            UserId = dto.UserId,            
            MethodOfPayment = dto.MethodOfPayment,
            SellerName = user.CompanyName,
            SellerNip = user.Nip,
            SellerAddress = FormatAddress(user.Address),
            BankAccountNumber = user.BankAccountNumber,
            ClientName = client.Name,
            ClientNip = client.Nip,
            ClientAddress = FormatAddress(client.Address),
            ClientEmail = dto.ClientEmail ?? string.Empty,
            InvoicePositions = [],
            Currency = dto.Currency,
        };
    }

    public static string FormatAddress(AddressDto address) =>
    address == null
        ? string.Empty
        : $"{address.Street} {address.Number}, {address.City}, {address.PostalCode}, {address.Country}";

    public static Invoice ToInvoiceWithExistingClient(this CreateInvoiceDto dto, Client client, User user)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "CreateInvoiceDto cannot be null when mapping to Invoice.");
        if (client == null)
            throw new ArgumentNullException(nameof(client), "Client cannot be null when mapping to Invoice.");
        if (user == null)
            throw new ArgumentNullException(nameof(user), "User cannot be null when mapping to Invoice.");

        return new Invoice
        {
            Title = dto.Title,
            TotalNet = dto.TotalNet,
            TotalVat = dto.TotalVat,
            TotalGross = dto.TotalGross,
            PaymentDate = dto.PaymentDate,
            CreatedDate = dto.CreatedDate,
            Comments = dto.Comments,
            UserId = dto.UserId,            
            MethodOfPayment = dto.MethodOfPayment,
            SellerName = user.CompanyName,
            SellerNip = user.Nip,
            SellerAddress = FormatAddress(user.Address),
            BankAccountNumber = user.BankAccountNumber,
            ClientName = client.Name,
            ClientNip = client.Nip,
            ClientAddress = FormatAddress(client.Address),
            ClientEmail = client.Email,
            InvoicePositions = [],
            Currency = dto.Currency
        };
    }

    public static string FormatAddress(Address address) =>
        address == null
            ? string.Empty
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
            invoice.TotalNet,
            invoice.TotalVat,
            invoice.TotalGross,
            invoice.PaymentDate,
            invoice.CreatedDate,
            invoice.Comments,
            null,
            invoice.UserId,
            null!,
            invoice.MethodOfPayment,
            invoice.InvoicePositions?
                .Select(ip => new UpdateInvoicePositionDto(
                    ip.InvoicePositionId,
                    ip.InvoiceId,
                    null,
                    ip.ProductName,
                    ip.ProductDescription,
                    ip.ProductValue,
                    ip.Quantity,
                    ip.VatRate,                    
                    null,
                    ip.UnitOfMeasure
                )).ToList() ?? [],
            invoice.SellerName,
            invoice.SellerNip,
            invoice.SellerAddress,
            invoice.BankAccountNumber,
            invoice.ClientName,
            invoice.ClientNip,
            invoice.ClientAddress,
            invoice.ClientEmail,
            invoice.Currency
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
            Email = dto.Email
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
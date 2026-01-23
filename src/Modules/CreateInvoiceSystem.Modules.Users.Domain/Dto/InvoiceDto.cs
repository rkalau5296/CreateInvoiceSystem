namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;
public record InvoiceDto(
    int InvoiceId,
    string Title,
    decimal TotalNet,
    decimal TotalVat,
    decimal TotalGross,
    DateTime PaymentDate,
    DateTime CreatedDate,
    string Comments,
    int? ClientId,
    int UserId,
    string MethodOfPayment,
    ICollection<InvoicePositionDto> InvoicePositions,
     string SellerName,
     string SellerNip,
     string SellerAddress,
     string BankAccountNumber,
     string ClientName,
     string ClientNip,
     string ClientAddress
);

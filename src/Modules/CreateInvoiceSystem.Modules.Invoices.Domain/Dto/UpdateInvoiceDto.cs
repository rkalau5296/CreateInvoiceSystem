namespace CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
public record UpdateInvoiceDto
(
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
    UpdateClientDto Client,
    string MethodOfPayment,
    ICollection<UpdateInvoicePositionDto> InvoicePositions,
    string SellerName,
    string SellerNip,
    string SellerAddress,
    string BankAccountNumber,
    string ClientName,
    string ClientNip,
    string ClientAddress
); 

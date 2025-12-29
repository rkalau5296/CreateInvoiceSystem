namespace CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
public record UpdateInvoiceDto
(
    int InvoiceId,
    string Title,
    decimal TotalAmount,
    DateTime PaymentDate,
    DateTime CreatedDate,
    string Comments,
    int? ClientId,
    int UserId,
    string MethodOfPayment,
    ICollection<UpdateInvoicePositionDto> InvoicePositions,
    string ClientName,
    string ClientNip,
    string ClientAddress
); 

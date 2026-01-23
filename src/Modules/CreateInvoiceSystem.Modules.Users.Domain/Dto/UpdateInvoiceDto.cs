namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;
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
    string MethodOfPayment,
    ICollection<UpdateInvoicePositionDto> InvoicePositions,
    string ClientName,
    string ClientNip,
    string ClientAddress
); 

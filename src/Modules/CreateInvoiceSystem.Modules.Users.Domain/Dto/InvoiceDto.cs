namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;
public record InvoiceDto(
    int InvoiceId,
    string Title,
    decimal TotalAmount,    
    DateTime PaymentDate,
    DateTime CreatedDate,
    string Comments,
    int? ClientId,
    int UserId,
    string MethodOfPayment,
    ICollection<InvoicePositionDto> InvoicePositions,
    string ClientName,
    string ClientNip,
    string ClientAddress
    );

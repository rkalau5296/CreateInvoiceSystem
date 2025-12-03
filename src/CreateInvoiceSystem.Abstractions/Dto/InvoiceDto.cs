using CreateInvoiceSystem.Abstractions.Entities;

namespace CreateInvoiceSystem.Abstractions.Dto;

public record InvoiceDto(
    int InvoiceId,
    string Title,
    decimal TotalAmount,    
    DateTime PaymentDate,
    DateTime CreatedDate,
    string Comments,
    int? ClientId,
    int UserId,    
    //ClientDto Client,
    //UserDto User,
    string MethodOfPayment,
    ICollection<InvoicePositionDto> InvoicePositionsDto
    );

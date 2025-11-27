using CreateInvoiceSystem.Abstractions.Entities;

namespace CreateInvoiceSystem.Abstractions.Dto;

public record InvoiceDto(
    int InvoiceId,
    string Title,
    decimal Value,
    //int MethodOfPaymentId,
    DateTime PaymentDate,
    DateTime CreatedDate,
    string Comments,
    int ClientId,
    int UserId,
    int ProductId,
    ProductDto Product,
    ClientDto Client,
    UserDto User
    );

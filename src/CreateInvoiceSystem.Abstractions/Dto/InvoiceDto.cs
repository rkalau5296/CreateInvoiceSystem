namespace CreateInvoiceSystem.Abstractions.Dto;

public record InvoiceDto(
    int InvoiceId,
    string Title,
    decimal Value,    
    DateTime PaymentDate,
    DateTime CreatedDate,
    string Comments,
    int ClientId,
    int UserId,
    //int ProductId,
    //ProductDto Product,
    ClientDto Client,
    UserDto User,
    string MethodOfPayment,
    ICollection<ProductDto> Products,
    string Product
    );

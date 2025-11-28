namespace CreateInvoiceSystem.Abstractions.Dto;

public record ProductDto(
    int ProductId,
    string Name,
    string Description,
    decimal Value,
    int UserId,
    UserDto UserDto,
    IEnumerable<InvoicePositionDto> InvoicePositions
);

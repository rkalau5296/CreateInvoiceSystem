namespace CreateInvoiceSystem.Abstractions.Dto;

public record ProductDto(
    int ProductId,
    string Name,
    decimal Value
);

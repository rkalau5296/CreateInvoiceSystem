namespace CreateInvoiceSystem.Abstractions.Dto;

public record CreateProductDto(
    string Name,
    string Description,
    decimal? Value,
    int UserId 
);

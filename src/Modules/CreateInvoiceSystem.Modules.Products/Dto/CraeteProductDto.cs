namespace CreateInvoiceSystem.Modules.Products.Domain.Dto;

public record CreateProductDto(
    string Name,
    string Description,
    decimal? Value,
    int UserId 
);

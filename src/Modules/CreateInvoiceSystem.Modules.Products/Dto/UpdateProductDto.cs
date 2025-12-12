namespace CreateInvoiceSystem.Modules.Products.Dto;

public record UpdateProductDto(
    int ProductId,
    string Name,
    string Description,
    decimal? Value,
    bool IsDeleted 
);

namespace CreateInvoiceSystem.Modules.Products.Domain.Dto;

public record UpdateProductDto(
    int ProductId,
    string Name,
    string Description,
    decimal? Value,
    int? UserId,
    bool IsDeleted 
);

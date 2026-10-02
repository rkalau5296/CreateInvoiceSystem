namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;

public record UpdateProductDto(
    int ProductId,
    string Name,
    string Description,
    decimal? Value,
    bool IsDeleted 
);

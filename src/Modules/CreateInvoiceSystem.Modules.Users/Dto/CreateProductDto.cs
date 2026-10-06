namespace CreateInvoiceSystem.Modules.Users.Dto;

public record CreateProductDto(
    string Name,
    string Description,
    decimal? Value,
    int UserId 
);

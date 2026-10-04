namespace CreateInvoiceSystem.Modules.Users.Dto;

public record ProductDto(
    int ProductId,
    string Name,
    string Description,
    decimal? Value,
    int UserId,
    bool IsDeleted
);
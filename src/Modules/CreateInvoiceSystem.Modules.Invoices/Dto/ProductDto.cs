namespace CreateInvoiceSystem.Modules.Invoices.Dto;
public record ProductDto(
    int ProductId,
    string Name,
    string Description,
    decimal? Value,
    int UserId  
);
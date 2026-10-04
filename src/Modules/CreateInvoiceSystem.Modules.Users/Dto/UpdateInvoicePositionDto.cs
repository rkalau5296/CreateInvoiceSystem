namespace CreateInvoiceSystem.Modules.Users.Dto;
public record UpdateInvoicePositionDto
(
    int InvoicePositionId,
    int InvoiceId,
    string ProductName,
    string ProductDescription,
    decimal? ProductValue,
    int Quantity
);

namespace CreateInvoiceSystem.Modules.InvoicePositions.Dto;

public record UpdateInvoicePositionDto
(
    int InvoicePositionId,
    int InvoiceId,
    string ProductName,
    string ProductDescription,
    decimal? ProductValue,
    int Quantity
);

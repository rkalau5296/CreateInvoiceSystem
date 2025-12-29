namespace CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
public record UpdateInvoicePositionDto
(
    int InvoicePositionId,
    int InvoiceId,
    string ProductName,
    string ProductDescription,
    decimal? ProductValue,
    int Quantity
);

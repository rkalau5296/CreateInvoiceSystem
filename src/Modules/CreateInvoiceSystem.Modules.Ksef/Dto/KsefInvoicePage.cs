namespace CreateInvoiceSystem.Modules.Ksef.Dto;

public sealed record KsefInvoicePage(IReadOnlyList<KsefInvoiceDto> Invoices, bool HasMore);

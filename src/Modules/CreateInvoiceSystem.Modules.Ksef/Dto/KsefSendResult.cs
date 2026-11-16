namespace CreateInvoiceSystem.Modules.Ksef.Dto;

public record KsefSendResult(
    string SessionReferenceNumber,
    string InvoiceReferenceNumber,
    string KsefNumber,
    string UpoXml);
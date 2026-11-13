namespace CreateInvoiceSystem.Modules.Ksef.Dto;

public record KsefOutboxStatus(int Status, string? KsefReferenceNumber, string? ErrorMessage);
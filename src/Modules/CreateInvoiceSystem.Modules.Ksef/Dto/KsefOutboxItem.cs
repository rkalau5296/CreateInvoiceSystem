namespace CreateInvoiceSystem.Modules.Ksef.Dto;

public record KsefOutboxItem(int Id, int UserId, int InvoiceId);
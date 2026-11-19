namespace CreateInvoiceSystem.Modules.Ksef.Dto;

public sealed class KsefInvoiceRejectedException(string message) : Exception(message);

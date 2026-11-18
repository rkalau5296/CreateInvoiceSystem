namespace CreateInvoiceSystem.Messaging.Contracts
{
    public record InvoiceCreatedIntegrationEvent(
        int InvoiceId,
        int UserId     
        );
}

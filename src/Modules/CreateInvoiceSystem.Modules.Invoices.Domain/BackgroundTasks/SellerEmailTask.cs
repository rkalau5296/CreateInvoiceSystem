namespace CreateInvoiceSystem.Modules.Invoices.Domain.BackgroundTasks
{
    public record SellerEmailTask(string UserEmail, string InvoiceTitle) : EmailTask;    
}

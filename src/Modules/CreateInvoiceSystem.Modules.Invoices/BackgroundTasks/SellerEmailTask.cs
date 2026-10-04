namespace CreateInvoiceSystem.Modules.Invoices.BackgroundTasks
{
    public record SellerEmailTask(string UserEmail, string InvoiceTitle) : EmailTask;    
}

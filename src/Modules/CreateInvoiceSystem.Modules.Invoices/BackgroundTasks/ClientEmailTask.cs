using CreateInvoiceSystem.Modules.Invoices.Dto;

namespace CreateInvoiceSystem.Modules.Invoices.BackgroundTasks
{
    public record ClientEmailTask(InvoiceDto Invoice) : EmailTask;    
}

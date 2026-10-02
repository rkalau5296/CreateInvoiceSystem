using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.BackgroundTasks
{
    public record ClientEmailTask(InvoiceDto Invoice) : EmailTask;    
}

using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.CloneInvoice;
using CreateInvoiceSystem.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.API.InvoiceCreatedBehavior;

/// <summary>
/// Zapisuje sklonowaną fakturę i uzupełnia w odpowiedzi jej InvoiceId.
/// Celowo nie publikuje InvoiceCreatedIntegrationEvent, żeby klon nie wysyłał
/// automatycznie e-maili z PDF do klienta, zanim użytkownik go sprawdzi.
/// </summary>
public class CloneInvoiceBehavior(CreateInvoiceSystemDbContext dbContext)
    : IPipelineBehavior<CloneInvoiceRequest, CloneInvoiceResponse>
{
    public async Task<CloneInvoiceResponse> Handle(
        CloneInvoiceRequest request,
        RequestHandlerDelegate<CloneInvoiceResponse> next,
        CancellationToken ct)
    {
        var response = await next(ct);

        await dbContext.SaveChangesAsync(ct);

        var invoiceId = await dbContext.Invoices
            .Where(i => i.UserId == response.Data.UserId && i.Title == response.Data.Title)
            .Select(i => i.InvoiceId)
            .SingleAsync(ct);

        response.Data = response.Data with { InvoiceId = invoiceId };
        return response;
    }
}

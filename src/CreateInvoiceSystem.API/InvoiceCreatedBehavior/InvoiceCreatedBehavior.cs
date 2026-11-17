using CreateInvoiceSystem.Messaging.Contracts;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.CreateInvoice;
using CreateInvoiceSystem.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.API.InvoiceCreatedBehavior;

public class InvoiceCreatedBehavior(
    CreateInvoiceSystemDbContext dbContext,
    IPublishEndpoint publishEndpoint)
    : IPipelineBehavior<CreateInvoiceRequest, CreateInvoiceResponse>
{
    public async Task<CreateInvoiceResponse> Handle(
    CreateInvoiceRequest request,
    RequestHandlerDelegate<CreateInvoiceResponse> next,
    CancellationToken ct)
    {
        var response = await next();

        var invoiceId = await dbContext.Invoices
            .Where(i => i.UserId == response.Data.UserId && i.Title == response.Data.Title)
            .Select(i => i.InvoiceId)
            .SingleAsync(ct);

        response.Data = response.Data with { InvoiceId = invoiceId };
        await publishEndpoint.Publish(
            new InvoiceCreatedIntegrationEvent(invoiceId, response.Data.UserId),
            ct);
        return response;
    }
}
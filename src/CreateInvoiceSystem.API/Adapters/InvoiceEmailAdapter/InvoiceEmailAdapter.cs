using CreateInvoiceSystem.Mail;
using CreateInvoiceSystem.Messaging.Contracts;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using MassTransit;

namespace CreateInvoiceSystem.API.Adapters.InvoiceEmailAdapter;

public class InvoiceEmailAdapter(
    IPublishEndpoint _publishEndpoint) : IInvoiceEmailSender
{
    public async Task SendInvoiceToClientCreatedAsync(InvoiceDto invoiceDto, CancellationToken cancellationToken)
    {
        await _publishEndpoint.Publish(new InvoiceCreatedIntegrationEvent(
            invoiceDto.InvoiceId,
            invoiceDto.UserId
            )
            , cancellationToken);
    }

    public Task SendInvoiceCreatedEmailAsync(string email, string invoiceNumber, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

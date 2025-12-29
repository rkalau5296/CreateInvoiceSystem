using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.DeleteInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
public class DeleteInvoiceHandler(ICommandExecutor commandExecutor, IInvoiceRepository _invoiceRepository) : IRequestHandler<DeleteInvoiceRequest, DeleteInvoiceResponse>
{
    public async Task<DeleteInvoiceResponse> Handle(DeleteInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = new Invoice { InvoiceId = request.Id };

        var command = new DeleteInvoiceCommand { Parametr = invoice };
        await commandExecutor.Execute(command, _invoiceRepository, cancellationToken);

        return new DeleteInvoiceResponse()
        {
            Data = InvoiceMappers.ToDto(invoice)
        };
    }
}


namespace CreateInvoiceSystem.Modules.Invoices.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using MediatR;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.DeleteInvoice;
using CreateInvoiceSystem.Modules.Invoices.Entities;
using CreateInvoiceSystem.Modules.Invoices.Application.Commands;
using CreateInvoiceSystem.Modules.Invoices.Mappers;

public class DeleteInvoiceHandler(ICommandExecutor commandExecutor) : IRequestHandler<DeleteInvoiceRequest, DeleteInvoiceResponse>
{
    public async Task<DeleteInvoiceResponse> Handle(DeleteInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = new Invoice { InvoiceId = request.Id };

        var command = new DeleteInvoiceCommand { Parametr = invoice };
        await commandExecutor.Execute(command, cancellationToken);

        return new DeleteInvoiceResponse()
        {
            Data = InvoiceMappers.ToDto(invoice)
        };
    }
}


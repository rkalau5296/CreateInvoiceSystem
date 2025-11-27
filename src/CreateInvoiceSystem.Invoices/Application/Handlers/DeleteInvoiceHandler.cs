namespace CreateInvoiceSystem.Invoices.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Invoices.Application.Commands;
using CreateInvoiceSystem.Abstractions.Mappers;
using CreateInvoiceSystem.Invoices.Application.RequestsResponses.DeleteInvoice;
using CreateInvoiceSystem.Abstractions.Entities;
using MediatR;

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


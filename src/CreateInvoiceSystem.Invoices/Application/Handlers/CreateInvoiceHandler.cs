namespace CreateInvoiceSystem.Invoices.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Invoices.Application.Commands;
using CreateInvoiceSystem.Invoices.Application.RequestsResponses.CreateInvoice;
using MediatR;

public class CreateInvoiceHandler(ICommandExecutor commandExecutor) : IRequestHandler<CreateInvoiceRequest, CreateInvoiceResponse>
{   
    public async Task<CreateInvoiceResponse> Handle(CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateInvoiceCommand() { Parametr = request.Invoice };

        var invoiceFromDb = await commandExecutor.Execute(command, cancellationToken);

        return new CreateInvoiceResponse()
        {
            Data = invoiceFromDb
        };
    }
}
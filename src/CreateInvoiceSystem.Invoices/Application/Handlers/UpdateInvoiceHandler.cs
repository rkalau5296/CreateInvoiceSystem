namespace CreateInvoiceSystem.Invoices.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Invoices.Application.Commands;
using CreateInvoiceSystem.Invoices.Application.RequestsResponses.UpdateInvoice;
using MediatR;


public class UpdateInvoiceHandler(ICommandExecutor commandExecutor) : IRequestHandler<UpdateInvoiceRequest, UpdateInvoiceResponse>
{    
    public async Task<UpdateInvoiceResponse> Handle(UpdateInvoiceRequest request, CancellationToken cancellationToken)
    {        
        var command = new UpdateInvoiceCommand() { Parametr = request.Invoice };
        
        var invoice = await commandExecutor.Execute(command, cancellationToken);        

        return new UpdateInvoiceResponse()
        {
            Data = invoice
        };
    }
}

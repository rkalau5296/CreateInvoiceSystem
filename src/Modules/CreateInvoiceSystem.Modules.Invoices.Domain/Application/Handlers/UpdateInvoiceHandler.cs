using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.UpdateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
public class UpdateInvoiceHandler(ICommandExecutor commandExecutor, IInvoiceRepository _invoiceRepository) : IRequestHandler<UpdateInvoiceRequest, UpdateInvoiceResponse>
{    
    public async Task<UpdateInvoiceResponse> Handle(UpdateInvoiceRequest request, CancellationToken cancellationToken)
    {        
        var command = new UpdateInvoiceCommand() { Parametr = request.Invoice };
        
        var invoice = await commandExecutor.Execute(command, _invoiceRepository, cancellationToken);        

        return new UpdateInvoiceResponse()
        {
            Data = invoice
        };
    }
}

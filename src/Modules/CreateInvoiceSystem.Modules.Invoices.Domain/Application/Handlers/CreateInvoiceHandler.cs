using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.CreateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
public class CreateInvoiceHandler(ICommandExecutor commandExecutor, IInvoiceRepository _invoiceRepository) : IRequestHandler<CreateInvoiceRequest, CreateInvoiceResponse>
{   
    public async Task<CreateInvoiceResponse> Handle(CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateInvoiceCommand() { Parametr = request.Invoice };

        var invoice = await commandExecutor.Execute(command, _invoiceRepository, cancellationToken);

        return new CreateInvoiceResponse()
        {
            Data = invoice
        };
    }
}
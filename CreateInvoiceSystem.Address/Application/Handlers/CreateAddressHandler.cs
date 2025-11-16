namespace CreateInvoiceSystem.Address.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Address.Application.Commands;
using CreateInvoiceSystem.Address.Application.RequestsResponses.CreateAddress;
using MediatR;

public class CreateAddressHandler(ICommandExecutor commandExecutor) : IRequestHandler<CreateAddressRequest, CreateAddressResponse>
{   
    public async Task<CreateAddressResponse> Handle(CreateAddressRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAddressCommand() { Parametr = request.Address };

        var addressFromDb = await commandExecutor.Execute(command, cancellationToken);

        return new CreateAddressResponse()
        {
            Data = addressFromDb
        };
    }
}
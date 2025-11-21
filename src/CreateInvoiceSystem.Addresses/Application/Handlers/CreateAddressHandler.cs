namespace CreateInvoiceSystem.Addresses.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Addresses.Application.Commands;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.CreateAddress;
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
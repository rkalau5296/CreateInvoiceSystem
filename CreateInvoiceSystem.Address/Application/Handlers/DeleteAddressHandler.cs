namespace CreateInvoiceSystem.Address.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Address.Application.Commands;
using CreateInvoiceSystem.Address.Domain.Entities;
using CreateInvoiceSystem.Address.Application.RequestsResponses.DeleteAddress;

using MediatR;

public class DeleteAddressHandler(ICommandExecutor commandExecutor) : IRequestHandler<DeleteAddressRequest, DeleteAddressResponse>
{
    private readonly ICommandExecutor commandExecutor = commandExecutor;

    public async Task<DeleteAddressResponse> Handle(DeleteAddressRequest request, CancellationToken cancellationToken)
    {
        var address = new Address { AddressId = request.Id };

        var command = new DeleteAddressCommand { Parametr = address };
        await commandExecutor.Execute(command, cancellationToken);
        return new DeleteAddressResponse();
    }
}


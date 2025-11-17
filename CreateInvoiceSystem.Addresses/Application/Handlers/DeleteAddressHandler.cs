namespace CreateInvoiceSystem.Addresses.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Addresses.Application.Commands;
using CreateInvoiceSystem.Addresses.Application.Mappers;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.DeleteAddress;
using CreateInvoiceSystem.Addresses.Domain.Entities;
using MediatR;

public class DeleteAddressHandler(ICommandExecutor commandExecutor) : IRequestHandler<DeleteAddressRequest, DeleteAddressResponse>
{
    public async Task<DeleteAddressResponse> Handle(DeleteAddressRequest request, CancellationToken cancellationToken)
    {
        var address = new Address { AddressId = request.Id };

        var command = new DeleteAddressCommand { Parametr = address };
        await commandExecutor.Execute(command, cancellationToken);

        return new DeleteAddressResponse()
        {
            Data = AddressMappers.ToDto(address)
        };
    }
}


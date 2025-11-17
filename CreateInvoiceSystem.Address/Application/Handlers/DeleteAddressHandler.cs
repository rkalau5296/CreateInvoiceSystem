namespace CreateInvoiceSystem.Address.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Address.Application.Commands;
using CreateInvoiceSystem.Address.Application.Mappers;
using CreateInvoiceSystem.Address.Application.RequestsResponses.DeleteAddress;
using CreateInvoiceSystem.Address.Domain.Entities;
using MediatR;
using System.Reflection.Metadata;

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


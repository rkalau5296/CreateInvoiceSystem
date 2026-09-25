using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.UpdateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
public class UpdateClientHandler(IClientRepository _clientRepository) : IRequestHandler<UpdateClientRequest, UpdateClientResponse>
{    
    public async Task<UpdateClientResponse> Handle(UpdateClientRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Client);

        var client = await _clientRepository.GetByIdAsync(request.Client.ClientId, request.Client.UserId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Client with ID {request.Client.ClientId} not found.");

        client.Name = request.Client.Name ?? client.Name;
        client.Nip = request.Client.Nip ?? client.Nip;
        client.Email = request.Client.Email ?? client.Email;

        if (client.Address is null && request.Client.Address is not null)
        {
            client.Address = new Address
            {
                Street = request.Client.Address.Street,
                Number = request.Client.Address.Number,
                City = request.Client.Address.City,
                PostalCode = request.Client.Address.PostalCode,
                Country = request.Client.Address.Country
            };
        }
        else if (client.Address is not null && request.Client.Address is not null)
        {
            client.Address.Street =
                request.Client.Address.Street ?? client.Address.Street;

            client.Address.Number =
                request.Client.Address.Number ?? client.Address.Number;

            client.Address.City =
                request.Client.Address.City ?? client.Address.City;

            client.Address.PostalCode =
                request.Client.Address.PostalCode ?? client.Address.PostalCode;

            client.Address.Country =
                request.Client.Address.Country ?? client.Address.Country;
        }

        var updatedClient = await _clientRepository.UpdateAsync(
            client,
            cancellationToken);        

        return new UpdateClientResponse()
        {
            Data = ClientMappers.ToUpdateDto(updatedClient)
        };
    }
}

using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Mappers;
public static class ClientMappers
{
    public static ClientDto ToDto(this Client client) =>
         client == null
        ? throw new ArgumentNullException(nameof(client), "Client cannot be null when mapping to ClientDto.")
        :
        new (client.ClientId, client.Name, client.Nip, client.Address.ToDto(), client.UserId, client.IsDeleted);

    public static UpdateClientDto ToUpdateDto(this Client client) =>
        client == null
        ? throw new ArgumentNullException(nameof(client), "Client cannot be null when mapping to UpdateClientDto.")
        :
        new(client.ClientId, client.Name, client.Nip, client.Address.ToDto(), client.AddressId, client.UserId);

    public static Client ToEntity(this ClientDto dto)
    {
        return dto == null
        ? throw new ArgumentNullException(nameof(dto), "Client cannot be null when mapping to Client.")
        :
        new()
        {
            ClientId = dto.ClientId,
            Name = dto.Name,
            Nip = dto.Nip,
            Address = dto.AddressDto.ToEntity(),
            UserId = dto.UserId,
            IsDeleted = dto.IsDeleted
        };
    }

    public static Client ToEntity(this CreateClientDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "Client cannot be null when mapping to Client.")
        :
        new()
        {            
            Name = dto.Name,
            Nip = dto.Nip,
            Address = dto.Address.ToEntity(),            
            UserId = dto.UserId,
            IsDeleted = dto.IsDeleted
        };

    public static CreateClientDto ToCreateDto(this Client client) =>
    client == null
        ? throw new ArgumentNullException(nameof(client), "Client cannot be null when mapping to CreateClientDto.")
        : new CreateClientDto(
            client.Name,
            client.Nip,
            client.Address.ToDto(), 
            client.UserId,
            client.IsDeleted
        );


    public static List<ClientDto> ToDtoList(this IEnumerable<Client> clients) =>
        clients == null
        ? throw new ArgumentNullException(nameof(clients), "Client's list cannot be null when mapping to ClientDto's list.")
        :
         [.. clients.Select(a => a.ToDto())];
}
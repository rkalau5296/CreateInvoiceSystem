namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class ClientMappers
{
    public static ClientDto ToDto(this Client client) =>
        new (client.ClientId, client.Name, client.Nip, client.Address?.ToDto(), client.UserId);

    public static UpdateClientDto ToUpdateDto(this Client client) =>
        new(client.ClientId, client.Name, client.Nip, client.Address?.ToDto(), client.AddressId, client.UserId);

    public static Client ToEntity(this ClientDto dto) =>
        new()
        {
            ClientId = dto.ClientId,
            Name = dto.Name,
            Nip = dto.Nip,           
            Address = dto.Address.ToEntity(),            
            UserId = dto.UserId,            
        };

    public static Client ToEntity(this CreateClientDto dto) =>
        new()
        {            
            Name = dto.Name,
            Nip = dto.Nip,
            Address = dto.Address.ToEntity(),            
            UserId = dto.UserId,
        };

    public static List<ClientDto> ToDtoList(this IEnumerable<Client> clients) =>
         [.. clients.Select(a => a.ToDto())];
}
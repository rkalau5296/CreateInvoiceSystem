namespace CreateInvoiceSystem.Clients.Application.Mappers;

using CreateInvoiceSystem.Abstractions.DTO;
using CreateInvoiceSystem.Abstractions.Entities;

public static class ClientMappers
{
    public static ClientDto ToDto(this Client client) =>
        new (client.ClientId, client.Name, client.AddressId, client.Email, client.UserId, client.Address);

    public static Client ToEntity(this ClientDto dto) =>
        new()
        {
            ClientId = dto.ClientId,
            Name = dto.Name,
            AddressId = dto.AddressId,
            Email = dto.Email,
            UserId = dto.UserId,
            Address = dto.Address 
        };

    public static List<ClientDto> ToDtoList(this IEnumerable<Client> addresses) =>
         [.. addresses.Select(a => a.ToDto())];
}

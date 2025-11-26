namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class ClientMappers
{
    public static ClientDto ToDto(this Client client) =>
        new (client.ClientId, client.Name, client.Nip, client.AddressId, client.Address?.ToDto(), client.UserId, client.User.ToDto());

    public static Client ToEntity(this ClientDto dto) =>
        new()
        {
            ClientId = dto.ClientId,
            Name = dto.Name,
            Nip = dto.Nip,
            AddressId = dto.AddressId,            
            Address = dto.AddressDto?.ToEntity(),
            UserId = dto.UserId,
            User = dto.UserDto.ToEntity()
        };

    public static List<ClientDto> ToDtoList(this IEnumerable<Client> clients) =>
         [.. clients.Select(a => a.ToDto())];
}
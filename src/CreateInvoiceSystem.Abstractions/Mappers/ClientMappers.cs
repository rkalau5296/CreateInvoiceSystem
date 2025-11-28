namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class ClientMappers
{
    public static ClientDto ToDto(this Client client) =>
        new (client.ClientId, client.Name, client.Nip, client.Address?.ToDto(), [.. client.Invoices.Select(i => i.ToDto())], client.UserId);

    public static Client ToEntity(this ClientDto dto) =>
        new()
        {
            ClientId = dto.ClientId,
            Name = dto.Name,
            Nip = dto.Nip,
            //AddressId = dto.AddressId,            
            Address = dto.Address.ToEntity(),
            Invoices = [.. dto.Invoices.Select(i => i.ToEntity())],
            UserId = dto.UserId,
            //User = dto.UserDto.ToEntity()
        };

    public static List<ClientDto> ToDtoList(this IEnumerable<Client> clients) =>
         [.. clients.Select(a => a.ToDto())];
}
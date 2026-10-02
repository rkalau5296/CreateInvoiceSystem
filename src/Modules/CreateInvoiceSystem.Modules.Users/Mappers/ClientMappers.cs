using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;

namespace CreateInvoiceSystem.Modules.Users.Domain.Mappers;
public static class ClientMappers
{
    public static ClientDto ToDto(this Client client) =>
         client == null
        ? throw new ArgumentNullException(nameof(client), "Client cannot be null when mapping to ClientDto.")
        :
        new (client.ClientId, client.Name, client.Nip, client.Address.ToDto(), client.UserId, client.Email);    

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
            Email = dto.Email
        };
    }    
}
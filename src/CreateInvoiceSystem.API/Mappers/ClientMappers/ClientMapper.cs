using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;

namespace CreateInvoiceSystem.API.Mappers.ClientMappers;

internal static class ClientMapper
{
    public static Client ToDomain(ClientEntity c, AddressEntity a)
    {
        return new Client
        {
            ClientId = c.ClientId,
            Name = c.Name,
            Nip = c.Nip,
            AddressId = c.AddressId,
            UserId = c.UserId,
            IsDeleted = c.IsDeleted,
            Address = a is null ? null : new Address
            {
                AddressId = a.AddressId,
                Street = a.Street,
                Number = a.Number,
                City = a.City,
                PostalCode = a.PostalCode,
                Country = a.Country
            }
        };
    }

    public static ClientEntity ToClientEntity(Client c)
    {
        return new ClientEntity
        {
            ClientId = c.ClientId,
            Name = c.Name,
            Nip = c.Nip,
            AddressId = c.AddressId,
            UserId = c.UserId,
            IsDeleted = c.IsDeleted
        };
    }

    public static AddressEntity ToAddressEntity(Address a)
    {
        return new AddressEntity
        {
            AddressId = a.AddressId,
            Street = a.Street,
            Number = a.Number,
            City = a.City,
            PostalCode = a.PostalCode,
            Country = a.Country
        };
    }
}
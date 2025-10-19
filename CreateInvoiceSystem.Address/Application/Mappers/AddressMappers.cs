namespace CreateInvoiceSystem.Address.Application.Mappers;

using CreateInvoiceSystem.Address.Application.DTO;
using CreateInvoiceSystem.Address.Domain.Entities;

public static class AddressMappers //do we need interfece?
{
    public static AddressDto ToDto(this Address e) =>
        new (e.Id, e.Street, e.Number, e.City, e.PostalCode, e.Email);
}

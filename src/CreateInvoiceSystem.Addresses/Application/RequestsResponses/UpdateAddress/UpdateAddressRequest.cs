namespace CreateInvoiceSystem.Addresses.Application.RequestsResponses.UpdateAddress;

using CreateInvoiceSystem.Abstractions.DTO;
using MediatR;

public class UpdateAddressRequest(int id, AddressDto addressDto) : IRequest<UpdateAddressResponse>
{
    public AddressDto Address { get; } = addressDto with { AddressId = id };
    public int Id { get; set; } = id;

}

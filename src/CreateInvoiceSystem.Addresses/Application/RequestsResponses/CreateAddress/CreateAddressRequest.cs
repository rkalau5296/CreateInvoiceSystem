namespace CreateInvoiceSystem.Addresses.Application.RequestsResponses.CreateAddress;

using CreateInvoiceSystem.Abstractions.DTO;
using MediatR;

public class CreateAddressRequest(AddressDto addressDto) : IRequest<CreateAddressResponse>
{
    public AddressDto Address { get; } = addressDto;
}

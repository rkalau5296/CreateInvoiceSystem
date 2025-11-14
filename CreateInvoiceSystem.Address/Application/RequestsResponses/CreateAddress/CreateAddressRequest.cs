namespace CreateInvoiceSystem.Address.Application.RequestsResponses.CreateAddress;

using CreateInvoiceSystem.Address.Application.DTO;
using MediatR;

public class CreateAddressRequest(AddressDto addressDto) : IRequest<CreateAddressResponse>
{
    public AddressDto Address { get; } = addressDto;
}

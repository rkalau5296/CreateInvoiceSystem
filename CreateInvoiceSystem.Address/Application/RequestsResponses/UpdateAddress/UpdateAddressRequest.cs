namespace CreateInvoiceSystem.Address.Application.RequestsResponses.UpdateAddress;

using CreateInvoiceSystem.Address.Application.DTO;
using MediatR;

public class PutAddressRequest(int id, AddressDto addressDto) : IRequest<PutAddressResponse>
{
    public AddressDto Address { get; } = addressDto;
    public int Id { get; set; } = id;
}

namespace CreateInvoiceSystem.Addresses.Application.RequestsResponses.DeleteAddress;

using MediatR;

public class DeleteAddressRequest(int id) : IRequest<DeleteAddressResponse>
{
    public int Id { get; } = id;
}

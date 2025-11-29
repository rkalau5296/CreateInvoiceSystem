namespace CreateInvoiceSystem.Clients.Application.RequestsResponses.UpdateClient;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class UpdateClientRequest(UpdateClientDto clientDto, int id) : IRequest<UpdateClientResponse>
{
    public UpdateClientDto Client { get; } = 
        (clientDto ?? throw new ArgumentNullException(nameof(clientDto),
            $"Argument '{nameof(clientDto)}' for client update request (Id={id}) cannot be null. Make sure the request body contains all required fields."
        )) with { ClientId = id };

    public int Id { get; } =
    id >= 1 ? id
        : throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than or equal to 1.");
}

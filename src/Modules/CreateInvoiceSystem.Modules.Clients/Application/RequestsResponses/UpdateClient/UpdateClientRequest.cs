using CreateInvoiceSystem.Modules.Clients.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.UpdateClient;

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

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Queries;
public class GetClientsQuery(int? userId) : QueryBase<List<Client>, IClientRepository>
{    
    public int? UserId { get; } = userId;

    public override async Task<List<Client>> Execute(IClientRepository _clientRepository, CancellationToken cancellationToken = default)
    {
        var clients = await _clientRepository.GetAllAsync(UserId, cancellationToken) ?? throw new InvalidOperationException("List of clients is empty.");

        var clientsList = clients.ToList();

        return clientsList.Count == 0
            ? throw new InvalidOperationException("List of clients is empty.")
            : clientsList;
    }
}
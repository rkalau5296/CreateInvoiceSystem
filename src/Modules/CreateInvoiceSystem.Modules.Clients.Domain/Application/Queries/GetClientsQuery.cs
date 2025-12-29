using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Queries;
public class GetClientsQuery : QueryBase<List<Client>, IClientRepository>
{
    public override async Task<List<Client>> Execute(IClientRepository _clientRepository, CancellationToken cancellationToken = default)
    {
        var clients = await _clientRepository.GetAllAsync(includeAddress: true, excludeDeleted: true, cancellationToken: cancellationToken);
        return clients.Count == 0 || clients is null
            ? throw new InvalidOperationException("List of clients is empty.")
            : clients;
    }
}
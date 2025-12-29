using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Queries;
public class GetClientQuery(int id) : QueryBase<Client, IClientRepository>
{
    public int Id { get; } = id;

    public override async Task<Client> Execute(IClientRepository _clientRepository, CancellationToken cancellationToken = default)
    {
        return await _clientRepository.GetByIdAsync(Id, includeAddress: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Client with ID {Id} not found.");
    }
}
using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Queries;
public class GetClientQuery(int id, int? userId) : QueryBase<Client, IClientRepository>
{
    public int Id { get; } = id;
    public int? UserId { get; } = userId; 

    public override async Task<Client> Execute(IClientRepository _clientRepository, CancellationToken cancellationToken = default)
    {        
        return await _clientRepository.GetByIdAsync(Id, UserId, cancellationToken)
            ?? throw new InvalidOperationException($"Client with ID {Id} not found or access denied.");
    }
}
using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Queries;
public class GetClientsQuery(int? userId, int pageNumber, int pageSize) : QueryBase<PagedResult<Client>, IClientRepository>
{    
    public int? UserId { get; } = userId;

    public override async Task<PagedResult<Client>> Execute(IClientRepository _clientRepository, CancellationToken cancellationToken = default)
    {
        var clients = await _clientRepository.GetAllAsync(UserId, pageNumber, pageSize, cancellationToken) ?? throw new InvalidOperationException("List of clients is empty.");

        var clientsList = clients.Items.ToList();

        return clients.TotalCount == 0
            ? throw new InvalidOperationException("List of clients is empty.")
            : clients;
    }
}
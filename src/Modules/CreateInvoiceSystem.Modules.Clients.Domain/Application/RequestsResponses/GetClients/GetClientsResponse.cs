using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClients;
public class GetClientsResponse : ResponseBase<List<ClientDto>>
{    
}
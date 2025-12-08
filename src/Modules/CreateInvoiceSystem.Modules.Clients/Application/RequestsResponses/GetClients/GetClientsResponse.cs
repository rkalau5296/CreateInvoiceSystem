using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Clients.Dto;

namespace CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.GetClients;

public class GetClientsResponse : ResponseBase<List<ClientDto>>
{    
}
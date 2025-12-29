using CreateInvoiceSystem.Abstractions.ControllerBase;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.CreateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.DeleteClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.UpdateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientController : ApiControllerBase
{
    public ClientController(IMediator mediator, ILogger<ClientController> logger) : base(mediator)
    {
        logger.LogInformation("This is ClientController");
    }

    [HttpGet("{clientId}")]
    [ProducesResponseType(typeof(GetClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetClientAsync([FromRoute] int clientId, CancellationToken cancellationToken)
    {
        GetClientRequest request = new(clientId);
        return HandleRequest<GetClientRequest, GetClientResponse>(request, cancellationToken);
    }

    [HttpGet()]
    [Route("/Clients")]
    public async Task<IActionResult> GetClientsAsync([FromQuery] GetClientsRequest request, CancellationToken cancellationToken)
    {
        return await HandleRequest<GetClientsRequest, GetClientsResponse>(request, cancellationToken);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateClientsAsync([FromBody] CreateClientDto clientDto, CancellationToken cancellationToken)
    {
        CreateClientRequest request = new(clientDto);
        return await HandleRequest<CreateClientRequest, CreateClientResponse>(request, cancellationToken);
    }

    [HttpPut]
    [Route("update/id")]
    public async Task<IActionResult> UpdateClientAsync(int id, [FromBody] UpdateClientDto clientDto, CancellationToken cancellationToken)
    {
        UpdateClientRequest request = new(clientDto, id);
        return await HandleRequest<UpdateClientRequest, UpdateClientResponse>(request, cancellationToken);
    }

    [HttpDelete]
    [Route("id")]
    public async Task<IActionResult> DeleteClient(int id, CancellationToken cancellationToken)
    {
        DeleteClientRequest request = new(id);
        return await HandleRequest<DeleteClientRequest, DeleteClientResponse>(request, cancellationToken);
    }
}

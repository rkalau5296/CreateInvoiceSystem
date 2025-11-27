namespace CreateInvoiceSystem.API.Controllers;

using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Clients.Application.RequestsResponses.CreateClient;
using CreateInvoiceSystem.Clients.Application.RequestsResponses.DeleteClient;
using CreateInvoiceSystem.Clients.Application.RequestsResponses.GetClient;
using CreateInvoiceSystem.Clients.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Clients.Application.RequestsResponses.UpdateClient;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
        return this.HandleRequest<GetClientRequest, GetClientResponse>(request, cancellationToken);
    }

    [HttpGet()]
    [Route("/Clients")]
    public async Task<IActionResult> GetClientsAsync([FromQuery] GetClientsRequest request, CancellationToken cancellationToken)
    {
        return await this.HandleRequest<GetClientsRequest, GetClientsResponse>(request, cancellationToken);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateClientsAsync([FromBody] ClientDto clientDto, CancellationToken cancellationToken)
    {
        CreateClientRequest request = new(clientDto);
        return await this.HandleRequest<CreateClientRequest, CreateClientResponse>(request, cancellationToken);
    }

    [HttpPut]
    [Route("update/id")]
    public async Task<IActionResult> UpdateClientAsync(int id, [FromBody] ClientDto clientDto, CancellationToken cancellationToken)
    {
        UpdateClientRequest request = new(id, clientDto);
        return await this.HandleRequest<UpdateClientRequest, UpdateClientResponse>(request, cancellationToken);
    }

    [HttpDelete]
    [Route("id")]
    public async Task<IActionResult> DeleteClient(int id, CancellationToken cancellationToken)
    {
        DeleteClientRequest request = new(id);
        return await this.HandleRequest<DeleteClientRequest, DeleteClientResponse>(request, cancellationToken);
    }
}

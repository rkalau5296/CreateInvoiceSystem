using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.CreateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.DeleteClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.UpdateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientController : ApiControllerBase
{
    public ClientController(IMediator mediator, ILogger<ClientController> logger) : base(mediator)
    {
        logger.LogInformation("This is ClientController");
    }

    [HttpGet("{clientId}")]
    [ProducesResponseType(typeof(GetClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientAsync([FromRoute] int clientId, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();
        
        GetClientRequest request = new(clientId) { UserId = actualUserId };
        return await HandleRequest<GetClientRequest, GetClientResponse>(request, cancellationToken);
    }

    [HttpGet]    
    public async Task<IActionResult> GetClientsAsync([FromQuery] GetClientsRequest request, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();
        
        request.UserId = actualUserId;

        return await HandleRequest<GetClientsRequest, GetClientsResponse>(request, cancellationToken);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateClientsAsync([FromBody] CreateClientDto clientDto, CancellationToken cancellationToken)
    {        
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();

        var secureDto = clientDto with { UserId = actualUserId };

        CreateClientRequest request = new(secureDto) { UserId = actualUserId };

        return await HandleRequest<CreateClientRequest, CreateClientResponse>(request, cancellationToken);
    }

    [HttpPut]
    [Route("update/{id}")]
    public async Task<IActionResult> UpdateClientAsync(int id, [FromBody] UpdateClientDto clientDto, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();
        
        var secureDto = clientDto with { UserId = actualUserId };
        
        UpdateClientRequest request = new(secureDto, id) { UserId = actualUserId };
        return await HandleRequest<UpdateClientRequest, UpdateClientResponse>(request, cancellationToken);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteClient(int id, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();
        
        DeleteClientRequest request = new(id) { UserId = actualUserId };
        return await HandleRequest<DeleteClientRequest, DeleteClientResponse>(request, cancellationToken);
    }
}

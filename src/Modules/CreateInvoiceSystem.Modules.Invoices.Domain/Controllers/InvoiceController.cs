using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.CreateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.DeleteInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoices;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.UpdateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class InvoiceController : ApiControllerBase
{
    public InvoiceController(IMediator mediator, ILogger<InvoiceController> logger) : base(mediator)
    {
        logger.LogInformation("This is InvoiceController");
    }

    [HttpGet("{invoiceId}")]
    [ProducesResponseType(typeof(GetInvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceAsync([FromRoute] int invoiceId, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();

        GetInvoiceRequest request = new(invoiceId) { UserId = actualUserId };
        return await HandleRequest<GetInvoiceRequest, GetInvoiceResponse>(request, cancellationToken);
    }

    [HttpGet()]
    [Route("/Invoices")]
    public async Task<IActionResult> GetInvoicesAsync([FromQuery] GetInvoicesRequest request, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();

        request.UserId = actualUserId;

        return await HandleRequest<GetInvoicesRequest, GetInvoicesResponse>(request, cancellationToken);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateInvoicesAsync([FromBody] CreateInvoiceDto invoiceDto, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();

        var secureDto = invoiceDto with { UserId = actualUserId };

        invoiceDto.UserEmail = User.FindFirstValue(ClaimTypes.Email);
        CreateInvoiceRequest request = new(secureDto) { UserId = actualUserId };

        return await HandleRequest<CreateInvoiceRequest, CreateInvoiceResponse>(request, cancellationToken);
    }

    [HttpPut]
    [Route("update/id")]
    public async Task<IActionResult> UpdateInvoiceAsync(int id, [FromBody] UpdateInvoiceDto updateInvoiceDto, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();

        var secureDto = updateInvoiceDto with { UserId = actualUserId };

        UpdateInvoiceRequest request = new(id, secureDto) { UserId = actualUserId };
        return await HandleRequest<UpdateInvoiceRequest, UpdateInvoiceResponse>(request, cancellationToken);
    }

    [HttpDelete]
    [Route("id")]
    public async Task<IActionResult> DeleteInvoice(int id, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();

        DeleteInvoiceRequest request = new(id) { UserId = actualUserId };
        return await HandleRequest<DeleteInvoiceRequest, DeleteInvoiceResponse>(request, cancellationToken);
    }
}
using System.Security.Claims;
using System.Text;
using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetIncomingKsefInvoices;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceDetails;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceStatus;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceXml;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoicePdf;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.SendInvoiceToKsef;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.KsefSettings;
using CreateInvoiceSystem.Modules.Ksef.Dto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreateInvoiceSystem.Modules.Ksef.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class KsefController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public KsefController(IMediator mediator) : base(mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettingsAsync(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out int userId)) return Unauthorized();
        Response.Headers.CacheControl = "no-store";
        var configured = await _mediator.Send(new GetKsefSettingsRequest(userId), cancellationToken);
        return Ok(new { IsConfigured = configured });
    }

    [HttpPut("settings")]
    public async Task<IActionResult> SaveSettingsAsync([FromBody] SaveKsefSettingsDto settings, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out int userId)) return Unauthorized();
        if (settings is null || string.IsNullOrWhiteSpace(settings.Token))
            return BadRequest("Podaj token KSeF.");
        Response.Headers.CacheControl = "no-store";
        await _mediator.Send(new SaveKsefSettingsRequest(userId, settings.Token), cancellationToken);
        return NoContent();
    }

    [HttpDelete("settings")]
    public async Task<IActionResult> DeleteSettingsAsync(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out int userId)) return Unauthorized();
        Response.Headers.CacheControl = "no-store";
        await _mediator.Send(new DeleteKsefSettingsRequest(userId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{invoiceId:int}/send")]
    public async Task<IActionResult> SendToKsefAsync([FromRoute] int invoiceId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out int userId)) return Unauthorized();

        var request = new SendInvoiceToKsefRequest(invoiceId) { UserId = userId };
        var outboxId = await _mediator.Send(request, cancellationToken);
        return Accepted(new { OutboxId = outboxId });
    }

    [HttpGet("{invoiceId:int}/status")]
    public async Task<IActionResult> GetStatusAsync([FromRoute] int invoiceId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out int userId)) return Unauthorized();

        var request = new GetKsefInvoiceStatusRequest(invoiceId) { UserId = userId };
        var status = await _mediator.Send(request, cancellationToken);
        return status is null ? NotFound() : Ok(status);
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> GetIncomingInvoicesAsync(
        [FromQuery] GetIncomingKsefInvoicesRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out int userId)) return Unauthorized();
        if (request.FromDate > request.ToDate
            || request.PageNumber < 1
            || request.PageSize is < 1 or > 100)
        {
            return BadRequest("Nieprawidłowy zakres dat lub parametry stronicowania.");
        }

        request.UserId = userId;
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("invoices/{ksefNumber}/xml")]
    public async Task<IActionResult> DownloadInvoiceXmlAsync(
        [FromRoute] string ksefNumber,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out int userId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(ksefNumber)) return BadRequest();

        var xml = await _mediator.Send(new GetKsefInvoiceXmlRequest(ksefNumber) { UserId = userId }, cancellationToken);
        var safeFileName = string.Concat(ksefNumber.Select(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_' or '.' ? character : '_'));
        return File(Encoding.UTF8.GetBytes(xml), "application/xml", $"{safeFileName}.xml");
    }

    [HttpGet("invoices/{ksefNumber}/pdf")]
    public async Task<IActionResult> GetInvoicePdfAsync([FromRoute] string ksefNumber, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out int userId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(ksefNumber)) return BadRequest();
        Response.Headers.CacheControl = "no-store";
        var pdf = await _mediator.Send(new GetKsefInvoicePdfRequest(userId, ksefNumber), cancellationToken);
        var safeFileName = string.Concat(ksefNumber.Select(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_' or '.' ? character : '_'));
        return File(pdf, "application/pdf", $"KSeF_{safeFileName}.pdf");
    }

    [HttpGet("invoices/{ksefNumber}")]
    public async Task<IActionResult> GetInvoiceDetailsAsync(
        [FromRoute] string ksefNumber,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out int userId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(ksefNumber)) return BadRequest();

        var response = await _mediator.Send(new GetKsefInvoiceDetailsRequest(ksefNumber) { UserId = userId }, cancellationToken);
        return Ok(response);
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId);
}

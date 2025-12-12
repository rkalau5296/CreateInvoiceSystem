using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.CreateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.DeleteInvoice;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetInvoice;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetInvoices;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.UpdateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Dto;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CreateInvoiceSystem.Modules.Invoices.Controllers;

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
    public Task<IActionResult> GetInvoiceAsync([FromRoute] int invoiceId, CancellationToken cancellationToken)
    {
        GetInvoiceRequest request = new(invoiceId);
        return this.HandleRequest<GetInvoiceRequest, GetInvoiceResponse>(request, cancellationToken);
    }

    [HttpGet()]
    [Route("/Invoices")]
    public async Task<IActionResult> GetInvoicesAsync([FromQuery] GetInvoicesRequest request, CancellationToken cancellationToken)
    {
        return await this.HandleRequest<GetInvoicesRequest, GetInvoicesResponse>(request, cancellationToken);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateInvoicesAsync([FromBody] CreateInvoiceDto invoiceDto, CancellationToken cancellationToken)
    {
        CreateInvoiceRequest request = new(invoiceDto);
        return await this.HandleRequest<CreateInvoiceRequest, CreateInvoiceResponse>(request, cancellationToken);
    }

    [HttpPut]
    [Route("update/id")]
    public async Task<IActionResult> UpdateInvoiceAsync(int id, [FromBody] InvoiceDto InvoiceDto, CancellationToken cancellationToken)
    {
        UpdateInvoiceRequest request = new(id, InvoiceDto);
        return await this.HandleRequest<UpdateInvoiceRequest, UpdateInvoiceResponse>(request, cancellationToken);
    }

    [HttpDelete]
    [Route("id")]
    public async Task<IActionResult> DeleteInvoice(int id, CancellationToken cancellationToken)
    {
        DeleteInvoiceRequest request = new(id);
        return await this.HandleRequest<DeleteInvoiceRequest, DeleteInvoiceResponse>(request, cancellationToken);
    }

}
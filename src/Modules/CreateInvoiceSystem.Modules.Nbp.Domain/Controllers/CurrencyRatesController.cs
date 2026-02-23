using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRate;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRates;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRate;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRates;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrencyRatesController : ApiControllerBase
{
    public CurrencyRatesController(IMediator mediator, ILogger<CurrencyRatesController> logger) : base(mediator)
    {
        logger.LogInformation("This is CurrencyRatesController");
    }

    [HttpGet]
    [Route("/CurrencyRates/{tableName}")]
    public async Task<IActionResult> GetCurencyRatesAsync([FromRoute] string tableName, CancellationToken cancellationToken)
    {
        GetActualCurrencyRatesRequest request = new(tableName);
        return await HandleRequest<GetActualCurrencyRatesRequest, GetActualCurrencyRatesResponse>(request, cancellationToken);
    }

    // removed :datetime constraint - parse inside
    [HttpGet]
    [Route("/CurrencyRates/{tableName}/{dateFrom}/{dateTo}")]
    public async Task<IActionResult> GetSeriesCurrencyRatesFromToAsync([FromRoute] string tableName, string dateFrom, string dateTo, CancellationToken cancellationToken)
    {
        if (!DateTime.TryParse(dateFrom, out var parsedFrom) || !DateTime.TryParse(dateTo, out var parsedTo))
        {
            var pd = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Title = "Invalid date format",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Date parameters must be valid dates (e.g. 2026-02-23 or ISO 8601)."
            };
            return BadRequest(pd);
        }

        GetSeriesCurrencyRatesFromToRequest request = new(tableName, parsedFrom, parsedTo);
        return await HandleRequest<GetSeriesCurrencyRatesFromToRequest, GetSeriesCurrencyRatesFromToResponse>(request, cancellationToken);
    }

    [HttpGet]
    [Route("/CurrencyRates/{tableName}/{currencyCode:alpha:length(3)}")]
    public async Task<IActionResult> GetCurencyRateAsync([FromRoute] string tableName, string currencyCode, CancellationToken cancellationToken)
    {
        GetActualCurrencyRateRequest request = new(tableName, currencyCode);
        return await HandleRequest<GetActualCurrencyRateRequest, GetActualCurrencyRateResponse>(request, cancellationToken);
    }

    [HttpGet]
    [Route("/CurrencyRates/{tableName}/{currencyCode:alpha:length(3)}/{dateFrom}/{dateTo}")]
    public async Task<IActionResult> GetSeriesCurrencyRateFromToAsync([FromRoute] string tableName, string currencyCode, string dateFrom, string dateTo, CancellationToken cancellationToken)
    {
        if (!DateTime.TryParse(dateFrom, out var parsedFrom) || !DateTime.TryParse(dateTo, out var parsedTo))
        {
            var pd = new ProblemDetails
            {
                Title = "Invalid date format",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Date parameters must be valid dates (e.g. 2026-02-23 or ISO 8601)."
            };
            return BadRequest(pd);
        }

        GetSeriesCurrencyRateFromToRequest request = new(tableName, currencyCode, parsedFrom, parsedTo);
        return await HandleRequest<GetSeriesCurrencyRateFromToRequest, GetSeriesCurrencyRateFromToResponse>(request, cancellationToken);
    }
}
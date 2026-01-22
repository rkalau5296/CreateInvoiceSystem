using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRate;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRates;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRate;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRates;
using MediatR;
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
    
    [HttpGet]
    [Route("/CurrencyRates/{tableName}/{dateFrom:datetime}/{dateTo:datetime}")]
    public async Task<IActionResult> GetSeriesCurrencyRatesFromToAsync([FromRoute] string tableName, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken)
    {
        GetSeriesCurrencyRatesFromToRequest request = new(tableName, dateFrom, dateTo);
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
    [Route("/CurrencyRates/{tableName}/{currencyCode:alpha:length(3)}/{dateFrom:datetime}/{dateTo:datetime}")]
    public async Task<IActionResult> GetSeriesCurrencyRateFromToAsync([FromRoute] string tableName, string currencyCode, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken)
    {
        GetSeriesCurrencyRateFromToRequest request = new(tableName, currencyCode, dateFrom, dateTo);
        return await HandleRequest<GetSeriesCurrencyRateFromToRequest, GetSeriesCurrencyRateFromToResponse>(request, cancellationToken);
    }
}

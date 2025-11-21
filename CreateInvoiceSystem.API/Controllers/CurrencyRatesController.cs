namespace CreateInvoiceSystem.API.Controllers;

using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Nbp.Application.RequestResponse.ActualRates;
using CreateInvoiceSystem.Nbp.Application.RequestResponse.PreviousDatesRates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class CurrencyRatesController: ApiControllerBase
{
    public CurrencyRatesController(IMediator mediator, ILogger<AddressController> logger) : base(mediator)
    {
        logger.LogInformation("This is CurrencyRatesController");
    }

    [HttpGet()]
    [Route("/CurrencyRates/{tableName}")]
    public async Task<IActionResult> GetCurencyRatesAsync([FromRoute] string tableName, CancellationToken cancellationToken)
    {
        GetActualCurrencyRatesRequest request = new(tableName);
        return await this.HandleRequest<GetActualCurrencyRatesRequest, GetActualCurrencyRatesResponse>(request, cancellationToken);
    }

    [HttpGet()]
    [Route("/CurrencyRates/{tableName}/{dateFrom}/{dateTo}")]
    public async Task<IActionResult> GetSeriesCurrencyRatesFromToAsync([FromRoute] string tableName, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken)
    {
        GetSeriesCurrencyRatesFromToRequest request = new(tableName, dateFrom, dateTo);
        return await this.HandleRequest<GetSeriesCurrencyRatesFromToRequest, GetSeriesCurrencyRatesFromToResponse>(request, cancellationToken);
    }
}

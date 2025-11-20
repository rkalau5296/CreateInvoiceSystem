namespace CreateInvoiceSystem.API.Controllers;

using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.GetAddresses;
using CreateInvoiceSystem.Nbp.Application.RequestResponse;
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
}

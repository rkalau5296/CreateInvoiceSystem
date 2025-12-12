using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Nbp.Application.DTO;

namespace CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.PreviousDatesRates;

public class GetSeriesCurrencyRatesFromToResponse : ResponseBase<List<CurrencyRatesTable>>
{
}

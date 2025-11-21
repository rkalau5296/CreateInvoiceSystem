using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Nbp.Application.DTO;

namespace CreateInvoiceSystem.Nbp.Application.RequestResponse.PreviousDatesRates;

public class GetSeriesCurrencyRatesFromToResponse : ResponseBase<List<CurrencyRatesTable>>
{
}

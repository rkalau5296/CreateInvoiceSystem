using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Nbp.Application.DTO;

namespace CreateInvoiceSystem.Nbp.Application.RequestResponse.ActualRates;

public class GetActualCurrencyRatesResponse : ResponseBase<List<CurrencyRatesTable>>
{

}

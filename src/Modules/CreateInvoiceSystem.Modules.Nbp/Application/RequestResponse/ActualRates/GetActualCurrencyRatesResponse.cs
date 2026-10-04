using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Nbp.Application.DTO;

namespace CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.ActualRates;
public class GetActualCurrencyRatesResponse : ResponseBase<List<CurrencyRatesTable>>
{

}

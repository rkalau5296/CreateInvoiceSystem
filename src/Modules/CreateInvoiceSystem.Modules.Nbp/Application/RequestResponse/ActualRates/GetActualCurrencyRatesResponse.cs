using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRates;
public class GetActualCurrencyRatesResponse : ResponseBase<List<CurrencyRatesTable>>
{

}

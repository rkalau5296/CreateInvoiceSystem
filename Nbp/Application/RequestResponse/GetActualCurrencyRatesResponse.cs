using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Nbp.Application.DTO;

namespace CreateInvoiceSystem.Nbp.Application.RequestResponse;

public class GetActualCurrencyRatesResponse : ResponseBase<List<CurrencyRatesTable>>
{

}

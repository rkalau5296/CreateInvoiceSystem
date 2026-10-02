using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRate;
public class GetActualCurrencyRateResponse : ResponseBase<CurrencyRatesTable>
{
}

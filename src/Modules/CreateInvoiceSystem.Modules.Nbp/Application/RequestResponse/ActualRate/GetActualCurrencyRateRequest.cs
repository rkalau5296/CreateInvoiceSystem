using MediatR;

namespace CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.ActualRate;
public class GetActualCurrencyRateRequest(string tableName, string currencyCode) : IRequest<GetActualCurrencyRateResponse>
{
    public string TableName { get; } = tableName;
    public string CurrencyCode { get; } = currencyCode;
}
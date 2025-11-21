namespace CreateInvoiceSystem.Nbp.Application.RequestResponse.ActualRate;

using MediatR;

public class GetActualCurrencyRateRequest(string tableName, string currencyCode) : IRequest<GetActualCurrencyRateResponse>
{
    public string TableName { get; } = tableName;
    public string CurrencyCode { get; } = currencyCode;
}

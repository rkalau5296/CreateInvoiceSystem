using MediatR;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRate;
public class GetSeriesCurrencyRateFromToRequest(string tableName, string currencyCode, DateTime dateFrom, DateTime dateTo ) : IRequest<GetSeriesCurrencyRateFromToResponse>
{
    public string TableName { get; set; } = tableName;
    public DateTime DateFrom { get; set; } = dateFrom;
    public DateTime DateTo { get; set; } = dateTo;
    public string CurrencyCode { get; set; } = currencyCode;
}

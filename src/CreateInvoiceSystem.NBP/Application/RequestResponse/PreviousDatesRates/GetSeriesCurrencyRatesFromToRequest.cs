namespace CreateInvoiceSystem.Nbp.Application.RequestResponse.PreviousDatesRates;

using MediatR;

public class GetSeriesCurrencyRatesFromToRequest(string tableName, DateTime dateFrom, DateTime dateTo ) : IRequest<GetSeriesCurrencyRatesFromToResponse>
{
    public string TableName { get; set; } = tableName;
    public DateTime DateFrom { get; set; } = dateFrom;
    public DateTime DateTo { get; set; } = dateTo;
}

namespace CreateInvoiceSystem.Nbp.Application.RequestResponse.ActualRates;

using MediatR;

public class GetActualCurrencyRatesRequest(string tableName) : IRequest<GetActualCurrencyRatesResponse>
{
    public string TableName { get; set; } = tableName;
	
}
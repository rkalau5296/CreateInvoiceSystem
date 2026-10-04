using MediatR;

namespace CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.ActualRates;
public class GetActualCurrencyRatesRequest(string tableName) : IRequest<GetActualCurrencyRatesResponse>
{
    public string TableName { get; set; } = tableName;
	
}
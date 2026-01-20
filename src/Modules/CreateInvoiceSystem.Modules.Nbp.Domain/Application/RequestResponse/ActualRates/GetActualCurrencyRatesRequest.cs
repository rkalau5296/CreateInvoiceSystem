using MediatR;

namespace CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRates;
public class GetActualCurrencyRatesRequest(string tableName) : IRequest<GetActualCurrencyRatesResponse>
{
    public string TableName { get; set; } = tableName;
	
}
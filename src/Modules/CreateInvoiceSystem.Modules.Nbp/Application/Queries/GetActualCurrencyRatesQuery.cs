namespace CreateInvoiceSystem.Modules.Nbp.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Nbp.Application.DTO;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

public class GetActualCurrencyRatesQuery(string table, string baseUrl) : QueryBase<List<CurrencyRatesTable>>
{    
    private readonly RestClient _client = new(baseUrl);   

    public override async Task<List<CurrencyRatesTable>> Execute(IDbContext context, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"tables/{table}/?format=json", Method.Get);        
        var response = await _client.ExecuteAsync<List<CurrencyRatesTable>>(request, cancellationToken: cancellationToken);
        
        if (!response.IsSuccessful || response.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"NBP API error: {response.StatusCode}");

        return JsonConvert.DeserializeObject<List<CurrencyRatesTable>>(response.Content); 
    }
}

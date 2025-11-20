namespace CreateInvoiceSystem.Nbp.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Nbp.Application.DTO;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

public class GetActualCurrencyRatesQuery(string table) : QueryBase<List<CurrencyRatesTable>>
{
    private const string baseUrl = "https://api.nbp.pl/api/exchangerates/";
    private readonly RestClient _client = new(baseUrl);   

    public override async Task<List<CurrencyRatesTable>> Execute(IDbContext context, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"tables/{table}/?format=json", Method.Get);        
        var response = await _client.ExecuteAsync<List<CurrencyRatesTable>>(request, cancellationToken: cancellationToken);

        if (!response.IsSuccessful || response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"NBP API error: {response.StatusCode}");

        return JsonConvert.DeserializeObject<List<CurrencyRatesTable>>(response.Content); 
    }
}

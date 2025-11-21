namespace CreateInvoiceSystem.Nbp.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Nbp.Application.DTO;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

public class GetSeriesCurrencyRatesFromToQuery(string table, DateTime dateFrom, DateTime dateTo) : QueryBase<List<CurrencyRatesTable>>
{
    private const string baseUrl = "https://api.nbp.pl/api/exchangerates/";
    private readonly RestClient _client = new(baseUrl);

    public override async Task<List<CurrencyRatesTable>> Execute(IDbContext context, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"tables/{table}/{dateFrom:yyyy-MM-dd}/{dateTo:yyyy-MM-dd}/?format=json", Method.Get);
        var response = await _client.ExecuteAsync<List<CurrencyRatesTable>>(request, cancellationToken: cancellationToken);

        if (!response.IsSuccessful || response.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"NBP API error: {response.StatusCode}");

        return JsonConvert.DeserializeObject<List<CurrencyRatesTable>>(response.Content);
    }
}

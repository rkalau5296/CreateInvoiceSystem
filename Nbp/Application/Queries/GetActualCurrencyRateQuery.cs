namespace CreateInvoiceSystem.Nbp.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Nbp.Application.DTO;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

public class GetActualCurrencyRateQyuery(string table, string currencyCode) : QueryBase<CurrencyRatesTable>
{
    private const string baseUrl = "https://api.nbp.pl/api/exchangerates/";
    private readonly RestClient _client = new(baseUrl);

    public override async Task<CurrencyRatesTable> Execute(IDbContext context, CancellationToken cancellationToken)
    {
        var request = new RestRequest($"rates/{table}/{currencyCode}/?format=json", Method.Get);
        var response = await _client.ExecuteAsync<CurrencyRatesTable>(request, cancellationToken: cancellationToken);

        if (!response.IsSuccessful || response.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"NBP API error: {response.StatusCode}");

        return JsonConvert.DeserializeObject<CurrencyRatesTable>(response.Content);
    }
}

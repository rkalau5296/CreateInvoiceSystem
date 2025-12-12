namespace CreateInvoiceSystem.Modules.Nbp.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Nbp.Application.DTO;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

public class GetActualCurrencyRateQyuery(string table, string currencyCode, string baseUrl) : QueryBase<CurrencyRatesTable>
{    
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

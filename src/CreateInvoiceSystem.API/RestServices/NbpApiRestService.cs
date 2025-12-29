using CreateInvoiceSystem.Modules.Nbp.Domain.Application.DTO;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Inerfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace CreateInvoiceSystem.API.RestServices
{
    public class NbpApiRestService : INbpApiRestService
    {
        private readonly RestClient _client;
        public NbpApiRestService(IOptions<NbpApiOptions> options, Exception argumentNullException)
        {
            var baseUrl = options.Value.BaseUrl ?? throw argumentNullException;
            _client = new RestClient(baseUrl);
        }

        public async Task<CurrencyRatesTable> GetActualCurrencyRateAsync(string baseUrl, string table, string currencyCode, CancellationToken cancellationToken)
        {            

            var request = new RestRequest($"rates/{table}/{currencyCode}/?format=json", Method.Get);
            var response = await _client.ExecuteAsync<CurrencyRatesTable>(request, cancellationToken);

            if (!response.IsSuccessful || response.StatusCode != HttpStatusCode.OK )
                throw new InvalidOperationException($"NBP API error: {response.StatusCode}");

            return JsonConvert.DeserializeObject< CurrencyRatesTable>(response.Content);
        }

        public async Task<List<CurrencyRatesTable>> GetActualCurrencyRatesAsync(string baseUrl, string table, CancellationToken cancellationToken)
        {
            var request = new RestRequest($"tables/{table}/?format=json", Method.Get);
            var response = await _client.ExecuteAsync<List<CurrencyRatesTable>>(request, cancellationToken: cancellationToken);

            if (!response.IsSuccessful || response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException($"NBP API error: {response.StatusCode}");

            return JsonConvert.DeserializeObject<List<CurrencyRatesTable>>(response.Content);
        }

        public async Task<CurrencyRatesTable> GetSeriesCurrencyRateFromToAsync(string baseUrl, string table, string currencyCode, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken)
        {
            var request = new RestRequest($"rates/{table}/{currencyCode}/{dateFrom:yyyy-MM-dd}/{dateTo:yyyy-MM-dd}/?format=json", Method.Get);
            var response = await _client.ExecuteAsync<CurrencyRatesTable>(request, cancellationToken: cancellationToken);

            if (!response.IsSuccessful || response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException($"NBP API error: {response.StatusCode}");

            return JsonConvert.DeserializeObject<CurrencyRatesTable>(response.Content);
        }

        public async Task<List<CurrencyRatesTable>> GetSeriesCurrencyRatesFromToAsync(string baseUrl, string table, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken)
        {
            var request = new RestRequest($"tables/{table}/{dateFrom:yyyy-MM-dd}/{dateTo:yyyy-MM-dd}/?format=json", Method.Get);
            var response = await _client.ExecuteAsync<List<CurrencyRatesTable>>(request, cancellationToken: cancellationToken);

            if (!response.IsSuccessful || response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException($"NBP API error: {response.StatusCode}");

            return JsonConvert.DeserializeObject<List<CurrencyRatesTable>>(response.Content);
        }
    }
}

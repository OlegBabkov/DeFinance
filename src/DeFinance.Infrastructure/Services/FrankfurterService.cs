using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DeFinance.Application.Abstractions;

namespace DeFinance.Infrastructure.Services;

public class FrankfurterService(IHttpClientFactory httpClientFactory) : IFrankfurterService
{
    public async Task<FrankfurterRatesResponse?> GetLatestAsync(CancellationToken ct = default)
    {
        var http = httpClientFactory.CreateClient("frankfurter");
        return await http.GetFromJsonAsync<FrankfurterApiResponse>("latest?from=EUR", ct)
            is { } r ? new FrankfurterRatesResponse(r.Base, r.Date, r.Rates) : null;
    }

    private record FrankfurterApiResponse(
        [property: JsonPropertyName("base")]  string Base,
        [property: JsonPropertyName("date")]  string Date,
        [property: JsonPropertyName("rates")] Dictionary<string, decimal> Rates);
}

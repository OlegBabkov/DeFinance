using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DeFinance.Application.Abstractions;

namespace DeFinance.Infrastructure.Services;

public class NbuService(IHttpClientFactory httpClientFactory) : INbuService
{
    public async Task<decimal?> GetUahPerEurAsync(CancellationToken ct = default)
    {
        var http = httpClientFactory.CreateClient("nbu");
        var rates = await http.GetFromJsonAsync<NbuRate[]>(
            "NBUStatService/v1/statdirectory/exchange?json", ct);

        return rates?.FirstOrDefault(r => r.Cc.Equals("EUR", StringComparison.OrdinalIgnoreCase))?.Rate;
    }

    private record NbuRate(
        [property: JsonPropertyName("cc")]   string Cc,
        [property: JsonPropertyName("rate")] decimal Rate);
}

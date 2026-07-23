namespace DeFinance.Application.Abstractions;

public record FrankfurterRatesResponse(string Base, string Date, Dictionary<string, decimal> Rates);

public interface IFrankfurterService
{
    Task<FrankfurterRatesResponse?> GetLatestAsync(CancellationToken ct = default);
}

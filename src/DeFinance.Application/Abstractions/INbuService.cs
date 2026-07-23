namespace DeFinance.Application.Abstractions;

public interface INbuService
{
    /// <summary>Returns UAH per 1 EUR from the NBU official rate, or null if unavailable.</summary>
    Task<decimal?> GetUahPerEurAsync(CancellationToken ct = default);
}

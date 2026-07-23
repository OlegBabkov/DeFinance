namespace DeFinance.Domain.Entities;

public class ExchangeRateHistory
{
    public Guid Id { get; private set; }
    public Guid CurrencyId { get; private set; }
    public Currency? Currency { get; private set; }
    public DateOnly Date { get; private set; }

    /// <summary>Units of this currency per 1 EUR (same convention as Transaction.ExchangeRate).</summary>
    public decimal Rate { get; private set; }

    private ExchangeRateHistory() { }

    public static ExchangeRateHistory Create(Guid currencyId, DateOnly date, decimal rate) =>
        new()
        {
            Id = Guid.NewGuid(),
            CurrencyId = currencyId,
            Date = date,
            Rate = rate
        };

    public void UpdateRate(decimal rate) => Rate = rate;
}

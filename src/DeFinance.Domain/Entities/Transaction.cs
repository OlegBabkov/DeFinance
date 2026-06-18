namespace DeFinance.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public DateTime DateTime { get; private set; }
    public decimal Sum { get; private set; }

    /// <summary>Exchange rate from account currency to the base reporting currency.</summary>
    public decimal ExchangeRate { get; private set; }

    /// <summary>Amount expressed in the base reporting currency (Sum / ExchangeRate).</summary>
    public decimal AmountInCurrency { get; private set; }

    public Guid InCurrencyId { get; private set; }
    public Currency? InCurrency { get; private set; }

    public Guid AccountId { get; private set; }
    public Account? Account { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public Guid? CounterpartyId { get; private set; }
    public Counterparty? Counterparty { get; private set; }

    public Guid PaymentStatusId { get; private set; }
    public PaymentStatus? PaymentStatus { get; private set; }

    public string? Notes { get; private set; }
    public Guid UserId { get; private set; }

    private Transaction() { }

    public static Transaction Create(
        DateTime dateTime,
        decimal sum,
        decimal exchangeRate,
        Guid inCurrencyId,
        Guid accountId,
        Guid categoryId,
        Guid? counterpartyId,
        Guid paymentStatusId,
        Guid userId,
        string? notes = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            DateTime = dateTime,
            Sum = sum,
            ExchangeRate = exchangeRate,
            AmountInCurrency = sum / exchangeRate,
            InCurrencyId = inCurrencyId,
            AccountId = accountId,
            CategoryId = categoryId,
            CounterpartyId = counterpartyId,
            PaymentStatusId = paymentStatusId,
            UserId = userId,
            Notes = notes
        };

    public void Update(
        DateTime dateTime,
        decimal sum,
        decimal exchangeRate,
        Guid inCurrencyId,
        Guid accountId,
        Guid categoryId,
        Guid? counterpartyId,
        Guid paymentStatusId,
        string? notes)
    {
        DateTime = dateTime;
        Sum = sum;
        ExchangeRate = exchangeRate;
        AmountInCurrency = sum / exchangeRate;
        InCurrencyId = inCurrencyId;
        AccountId = accountId;
        CategoryId = categoryId;
        CounterpartyId = counterpartyId;
        PaymentStatusId = paymentStatusId;
        Notes = notes;
    }
}

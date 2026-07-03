namespace DeFinance.Domain.Entities;

public class MandatoryPayment
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public Guid CurrencyId { get; private set; }
    public Currency? Currency { get; private set; }
    public Guid AccountId { get; private set; }
    public Account? Account { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public Guid? PaymentStatusId { get; private set; }
    public PaymentStatus? PaymentStatus { get; private set; }
    public PaymentFrequency Frequency { get; private set; }
    public int DayOfPeriod { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }
    public Guid UserId { get; private set; }

    private MandatoryPayment() { }

    public static MandatoryPayment Create(
        string name,
        decimal amount,
        Guid currencyId,
        Guid accountId,
        Guid? categoryId,
        Guid? paymentStatusId,
        PaymentFrequency frequency,
        int dayOfPeriod,
        string? notes,
        Guid userId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Amount = amount,
            CurrencyId = currencyId,
            AccountId = accountId,
            CategoryId = categoryId,
            PaymentStatusId = paymentStatusId,
            Frequency = frequency,
            DayOfPeriod = dayOfPeriod,
            Notes = notes,
            UserId = userId,
            IsActive = true,
        };

    public void Update(
        string name,
        decimal amount,
        Guid currencyId,
        Guid accountId,
        Guid? categoryId,
        Guid? paymentStatusId,
        PaymentFrequency frequency,
        int dayOfPeriod,
        string? notes)
    {
        Name = name;
        Amount = amount;
        CurrencyId = currencyId;
        AccountId = accountId;
        CategoryId = categoryId;
        PaymentStatusId = paymentStatusId;
        Frequency = frequency;
        DayOfPeriod = dayOfPeriod;
        Notes = notes;
    }

    public void UpdatePaymentStatus(Guid? paymentStatusId) => PaymentStatusId = paymentStatusId;

    public void Activate()   => IsActive = true;
    public void Deactivate() => IsActive = false;
}

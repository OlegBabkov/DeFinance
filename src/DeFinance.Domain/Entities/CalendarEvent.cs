namespace DeFinance.Domain.Entities;

public class CalendarEvent
{
    public Guid Id { get; private set; }
    public DateOnly Date { get; private set; }
    public string EventType { get; private set; } = string.Empty; // "Event" | "Payment"

    // Event-only fields
    public string? Title { get; private set; }
    public TimeOnly? TimeFrom { get; private set; }
    public TimeOnly? TimeTo { get; private set; }

    // Payment-only fields
    public Guid? AccountId { get; private set; }
    public Account? Account { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public Guid? CounterpartyId { get; private set; }
    public Counterparty? Counterparty { get; private set; }
    public Guid? PaymentStatusId { get; private set; }
    public PaymentStatus? PaymentStatus { get; private set; }
    public Guid? InCurrencyId { get; private set; }
    public decimal? Sum { get; private set; }
    public decimal? ExchangeRate { get; private set; }

    // Common
    public string? Notes { get; private set; }
    public Guid UserId { get; private set; }

    private CalendarEvent() { }

    public static CalendarEvent CreateEvent(
        DateOnly date, string? title, TimeOnly? timeFrom, TimeOnly? timeTo,
        Guid userId, string? notes = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Date = date,
            EventType = "Event",
            Title = title,
            TimeFrom = timeFrom,
            TimeTo = timeTo,
            UserId = userId,
            Notes = notes
        };

    public static CalendarEvent CreatePayment(
        DateOnly date, Guid accountId, Guid categoryId, Guid? counterpartyId,
        Guid paymentStatusId, Guid inCurrencyId, decimal sum, decimal exchangeRate,
        Guid userId, string? notes = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Date = date,
            EventType = "Payment",
            AccountId = accountId,
            CategoryId = categoryId,
            CounterpartyId = counterpartyId,
            PaymentStatusId = paymentStatusId,
            InCurrencyId = inCurrencyId,
            Sum = sum,
            ExchangeRate = exchangeRate,
            UserId = userId,
            Notes = notes
        };
}

namespace DeFinance.Application.DTOs.CalendarEvents;

public record CalendarEventResponse(
    Guid Id,
    string Date,              // YYYY-MM-DD
    string EventType,         // "Event" | "Payment"
    // Event fields
    string? Title,
    string? TimeFrom,         // HH:mm
    string? TimeTo,           // HH:mm
    // Payment fields
    Guid? AccountId,
    string? AccountName,
    string? AccountCurrencySymbol,
    Guid? CategoryId,
    string? CategoryName,
    string? CategoryColor,
    string? CategoryIcon,
    Guid? CounterpartyId,
    string? CounterpartyName,
    Guid? PaymentStatusId,
    string? PaymentStatusName,
    string? PaymentStatusColor,
    Guid? InCurrencyId,
    decimal? Sum,
    decimal? ExchangeRate,
    // Common
    string? Color,
    string? Notes);

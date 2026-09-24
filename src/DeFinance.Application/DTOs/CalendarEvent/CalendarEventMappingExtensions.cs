using DeFinance.Domain.Entities;

namespace DeFinance.Application.DTOs.CalendarEvents;

public static class CalendarEventMappingExtensions
{
    public static CalendarEventResponse ToResponse(this CalendarEvent e) =>
        new(
            e.Id,
            e.Date.ToString("yyyy-MM-dd"),
            e.EventType,
            e.Title,
            e.TimeFrom?.ToString("HH:mm"),
            e.TimeTo?.ToString("HH:mm"),
            e.AccountId,
            e.Account?.Name,
            e.Account?.Currency?.Symbol,
            e.CategoryId,
            e.Category?.Name,
            e.Category?.Color,
            e.Category?.Icon,
            e.CounterpartyId,
            e.Counterparty?.Name,
            e.PaymentStatusId,
            e.PaymentStatus?.Name,
            e.PaymentStatus?.Color,
            e.InCurrencyId,
            e.Sum,
            e.ExchangeRate,
            e.Color,
            e.Notes);
}

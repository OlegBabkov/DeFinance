using DeFinance.Domain.Entities;

namespace DeFinance.Application.Abstractions.Repositories;

public interface ICalendarEventRepository
{
    Task<IReadOnlyList<CalendarEvent>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CalendarEvent>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CalendarEvent?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default);
    void Remove(CalendarEvent calendarEvent);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

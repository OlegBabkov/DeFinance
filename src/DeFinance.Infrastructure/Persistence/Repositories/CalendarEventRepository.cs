using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Repositories;

public class CalendarEventRepository(DeFinanceDbContext dbContext, ICurrentUserService currentUserService) : ICalendarEventRepository
{
    private readonly Guid _userId = currentUserService.UserId;

    public async Task<IReadOnlyList<CalendarEvent>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default) =>
        await dbContext.CalendarEvents
            .Where(e => e.UserId == _userId && e.Date == date)
            .Include(e => e.Account).ThenInclude(a => a!.Currency)
            .Include(e => e.Category)
            .Include(e => e.Counterparty)
            .Include(e => e.PaymentStatus)
            .OrderBy(e => e.EventType)
            .ThenBy(e => e.TimeFrom)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CalendarEvent>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default) =>
        await dbContext.CalendarEvents
            .Where(e => e.UserId == _userId && e.Date >= from && e.Date <= to)
            .Include(e => e.Account).ThenInclude(a => a!.Currency)
            .Include(e => e.Category)
            .Include(e => e.Counterparty)
            .Include(e => e.PaymentStatus)
            .OrderBy(e => e.Date)
            .ThenBy(e => e.EventType)
            .ThenBy(e => e.TimeFrom)
            .ToListAsync(cancellationToken);

    public async Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.CalendarEvents
            .Where(e => e.UserId == _userId && e.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<CalendarEvent?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.CalendarEvents
            .Where(e => e.UserId == _userId && e.Id == id)
            .Include(e => e.Account).ThenInclude(a => a!.Currency)
            .Include(e => e.Category)
            .Include(e => e.Counterparty)
            .Include(e => e.PaymentStatus)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default) =>
        await dbContext.CalendarEvents.AddAsync(calendarEvent, cancellationToken);

    public void Remove(CalendarEvent calendarEvent) =>
        dbContext.CalendarEvents.Remove(calendarEvent);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

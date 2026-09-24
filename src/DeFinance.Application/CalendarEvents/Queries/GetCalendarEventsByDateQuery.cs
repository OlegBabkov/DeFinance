using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.DTOs.CalendarEvents;
using MediatR;

namespace DeFinance.Application.CalendarEvents.Queries;

public record GetCalendarEventsByDateQuery(DateOnly Date, int Page, int PageSize)
    : IRequest<PagedResult<CalendarEventResponse>>;

public class GetCalendarEventsByDateQueryHandler(ICalendarEventRepository calendarEventRepository)
    : IRequestHandler<GetCalendarEventsByDateQuery, PagedResult<CalendarEventResponse>>
{
    public async Task<PagedResult<CalendarEventResponse>> Handle(
        GetCalendarEventsByDateQuery request, CancellationToken cancellationToken)
    {
        var items = await calendarEventRepository.GetByDateAsync(request.Date, cancellationToken);
        var responses = items.Select(e => e.ToResponse()).ToList();
        return new PagedResult<CalendarEventResponse>(responses, responses.Count, request.Page, request.PageSize);
    }
}

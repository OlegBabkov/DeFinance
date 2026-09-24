using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.DTOs.CalendarEvents;
using MediatR;

namespace DeFinance.Application.CalendarEvents.Queries;

public record GetCalendarEventsByDateRangeQuery(DateOnly From, DateOnly To, int Page, int PageSize)
    : IRequest<PagedResult<CalendarEventResponse>>;

public class GetCalendarEventsByDateRangeQueryHandler(ICalendarEventRepository calendarEventRepository)
    : IRequestHandler<GetCalendarEventsByDateRangeQuery, PagedResult<CalendarEventResponse>>
{
    public async Task<PagedResult<CalendarEventResponse>> Handle(
        GetCalendarEventsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var items = await calendarEventRepository.GetByDateRangeAsync(request.From, request.To, cancellationToken);
        var responses = items.Select(e => e.ToResponse()).ToList();
        return new PagedResult<CalendarEventResponse>(responses, responses.Count, request.Page, request.PageSize);
    }
}

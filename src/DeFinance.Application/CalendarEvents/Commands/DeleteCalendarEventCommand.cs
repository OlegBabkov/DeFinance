using DeFinance.Application.Abstractions.Repositories;
using MediatR;

namespace DeFinance.Application.CalendarEvents.Commands;

public record DeleteCalendarEventCommand(Guid Id) : IRequest<bool>;

public class DeleteCalendarEventCommandHandler(ICalendarEventRepository calendarEventRepository)
    : IRequestHandler<DeleteCalendarEventCommand, bool>
{
    public async Task<bool> Handle(DeleteCalendarEventCommand request, CancellationToken cancellationToken)
    {
        var calendarEvent = await calendarEventRepository.GetByIdAsync(request.Id, cancellationToken);
        if (calendarEvent is null) return false;

        calendarEventRepository.Remove(calendarEvent);
        await calendarEventRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}

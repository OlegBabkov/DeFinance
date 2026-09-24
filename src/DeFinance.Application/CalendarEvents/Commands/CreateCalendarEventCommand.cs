using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.CalendarEvents;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.CalendarEvents.Commands;

public record CreateCalendarEventCommand(
    string Date,
    string EventType,
    string? Title,
    string? TimeFrom,
    string? TimeTo,
    Guid? AccountId,
    Guid? CategoryId,
    Guid? CounterpartyId,
    Guid? PaymentStatusId,
    Guid? InCurrencyId,
    decimal? Sum,
    decimal? ExchangeRate,
    string? Color,
    string? Notes
) : IRequest<CalendarEventResponse>;

public class CreateCalendarEventCommandHandler(
    ICalendarEventRepository calendarEventRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateCalendarEventCommand, CalendarEventResponse>
{
    public async Task<CalendarEventResponse> Handle(
        CreateCalendarEventCommand request, CancellationToken cancellationToken)
    {
        var date = DateOnly.Parse(request.Date);
        var userId = currentUserService.UserId;

        CalendarEvent calendarEvent;

        if (request.EventType == "Event")
        {
            var timeFrom = string.IsNullOrEmpty(request.TimeFrom) ? (TimeOnly?)null : TimeOnly.Parse(request.TimeFrom);
            var timeTo = string.IsNullOrEmpty(request.TimeTo) ? (TimeOnly?)null : TimeOnly.Parse(request.TimeTo);
            calendarEvent = CalendarEvent.CreateEvent(date, request.Title, timeFrom, timeTo, userId, request.Color, request.Notes);
        }
        else
        {
            calendarEvent = CalendarEvent.CreatePayment(
                date, request.AccountId!.Value, request.CategoryId!.Value,
                request.CounterpartyId, request.PaymentStatusId!.Value,
                request.InCurrencyId!.Value, request.Sum!.Value, request.ExchangeRate!.Value,
                userId, request.Color, request.Notes);
        }

        await calendarEventRepository.AddAsync(calendarEvent, cancellationToken);
        await calendarEventRepository.SaveChangesAsync(cancellationToken);

        return (await calendarEventRepository.GetByIdWithDetailsAsync(calendarEvent.Id, cancellationToken))!.ToResponse();
    }
}

public class CreateCalendarEventCommandValidator : AbstractValidator<CreateCalendarEventCommand>
{
    public CreateCalendarEventCommandValidator()
    {
        RuleFor(x => x.Date).NotEmpty().Must(d => DateOnly.TryParse(d, out _)).WithMessage("Invalid date format.");
        RuleFor(x => x.EventType).Must(t => t is "Event" or "Payment").WithMessage("EventType must be 'Event' or 'Payment'.");
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes is not null);

        When(x => x.EventType == "Payment", () =>
        {
            RuleFor(x => x.AccountId).NotEmpty().WithMessage("Account is required for a Payment event.");
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category is required for a Payment event.");
            RuleFor(x => x.PaymentStatusId).NotEmpty().WithMessage("Payment status is required for a Payment event.");
            RuleFor(x => x.InCurrencyId).NotEmpty().WithMessage("Currency is required for a Payment event.");
            RuleFor(x => x.Sum).GreaterThan(0).WithMessage("Sum must be greater than zero.");
            RuleFor(x => x.ExchangeRate).GreaterThan(0).WithMessage("Exchange rate must be greater than zero.");
        });
    }
}

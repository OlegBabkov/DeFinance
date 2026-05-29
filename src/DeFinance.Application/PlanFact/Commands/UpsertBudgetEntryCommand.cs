using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.PlanFact.Commands;

public record UpsertBudgetEntryCommand(
    Guid CategoryId,
    int Year,
    int Month,
    decimal PlannedAmount
) : IRequest<Unit>;

public class UpsertBudgetEntryCommandHandler(IBudgetEntryRepository repository)
    : IRequestHandler<UpsertBudgetEntryCommand, Unit>
{
    public async Task<Unit> Handle(UpsertBudgetEntryCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetAsync(request.CategoryId, request.Year, request.Month, cancellationToken);
        if (existing is not null)
        {
            existing.UpdateAmount(request.PlannedAmount);
        }
        else
        {
            var entry = BudgetEntry.Create(request.CategoryId, request.Year, request.Month, request.PlannedAmount);
            await repository.AddAsync(entry, cancellationToken);
        }
        await repository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public class UpsertBudgetEntryCommandValidator : AbstractValidator<UpsertBudgetEntryCommand>
{
    public UpsertBudgetEntryCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(2000, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.PlannedAmount).GreaterThanOrEqualTo(0);
    }
}

using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.PlanFact.Commands;

public record BudgetEntryLineRequest(string Name, decimal Amount);

public record UpsertBudgetEntryCommand(
    Guid CategoryId,
    int Year,
    int Month,
    decimal PlannedAmount,
    IReadOnlyList<BudgetEntryLineRequest> Lines
) : IRequest<Unit>;

public class UpsertBudgetEntryCommandHandler(IBudgetEntryRepository repository, ICurrentUserService currentUserService)
    : IRequestHandler<UpsertBudgetEntryCommand, Unit>
{
    public async Task<Unit> Handle(UpsertBudgetEntryCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetAsync(request.CategoryId, request.Year, request.Month, cancellationToken);
        var lines = request.Lines.Select(l => (l.Name, l.Amount));
        if (existing is not null)
        {
            await repository.UpdateDirectAsync(existing.Id, request.PlannedAmount, lines, cancellationToken);
        }
        else
        {
            var entry = BudgetEntry.Create(request.CategoryId, request.Year, request.Month, request.PlannedAmount, currentUserService.UserId);
            entry.UpdateLines(lines);
            await repository.AddAsync(entry, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
        }
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

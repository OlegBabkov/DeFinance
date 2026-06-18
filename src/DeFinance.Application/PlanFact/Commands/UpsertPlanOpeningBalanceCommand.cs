using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.PlanFact.Commands;

public record UpsertPlanOpeningBalanceCommand(int Year, int Month, decimal Amount) : IRequest<Unit>;

public class UpsertPlanOpeningBalanceCommandHandler(IOpeningBalanceOverrideRepository repository, ICurrentUserService currentUserService)
    : IRequestHandler<UpsertPlanOpeningBalanceCommand, Unit>
{
    public async Task<Unit> Handle(UpsertPlanOpeningBalanceCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetAsync(request.Year, request.Month, cancellationToken);
        if (existing is not null)
        {
            existing.UpdatePlanAmount(request.Amount);
        }
        else
        {
            await repository.AddAsync(
                OpeningBalanceOverride.CreateForPlan(request.Year, request.Month, request.Amount, currentUserService.UserId),
                cancellationToken);
        }
        await repository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public class UpsertPlanOpeningBalanceCommandValidator : AbstractValidator<UpsertPlanOpeningBalanceCommand>
{
    public UpsertPlanOpeningBalanceCommandValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}

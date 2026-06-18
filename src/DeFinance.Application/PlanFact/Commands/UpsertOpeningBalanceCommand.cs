using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.PlanFact.Commands;

public record UpsertOpeningBalanceCommand(int Year, int Month, decimal Amount) : IRequest<Unit>;

public class UpsertOpeningBalanceCommandHandler(IOpeningBalanceOverrideRepository repository, ICurrentUserService currentUserService)
    : IRequestHandler<UpsertOpeningBalanceCommand, Unit>
{
    public async Task<Unit> Handle(UpsertOpeningBalanceCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetAsync(request.Year, request.Month, cancellationToken);
        if (existing is not null)
        {
            existing.UpdateAmount(request.Amount);
        }
        else
        {
            await repository.AddAsync(
                OpeningBalanceOverride.Create(request.Year, request.Month, request.Amount, currentUserService.UserId!.Value),
                cancellationToken);
        }
        await repository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public class UpsertOpeningBalanceCommandValidator : AbstractValidator<UpsertOpeningBalanceCommand>
{
    public UpsertOpeningBalanceCommandValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}

using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Counterparty;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Counterparties.Commands;

public record UpdateCounterpartyCommand(Guid Id, string Name, CounterpartyType Type, string? ContactInfo)
    : IRequest<CounterpartyResponse?>;

public class UpdateCounterpartyCommandHandler(ICounterpartyRepository repository)
    : IRequestHandler<UpdateCounterpartyCommand, CounterpartyResponse?>
{
    public async Task<CounterpartyResponse?> Handle(UpdateCounterpartyCommand request, CancellationToken cancellationToken)
    {
        var counterparty = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (counterparty is null) return null;

        counterparty.Update(request.Name, request.Type, request.ContactInfo);
        await repository.SaveChangesAsync(cancellationToken);
        return counterparty.ToResponse();
    }
}

public class UpdateCounterpartyCommandValidator : AbstractValidator<UpdateCounterpartyCommand>
{
    public UpdateCounterpartyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Counterparty name is required.")
            .MaximumLength(100).WithMessage("Counterparty name must not exceed 100 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Counterparty type is invalid.");

        RuleFor(x => x.ContactInfo)
            .MaximumLength(500).WithMessage("Contact info must not exceed 500 characters.")
            .When(x => x.ContactInfo is not null);
    }
}

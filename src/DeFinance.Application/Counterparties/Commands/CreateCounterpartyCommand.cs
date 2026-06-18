using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Counterparty;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Counterparties.Commands;

public record CreateCounterpartyCommand(string Name, CounterpartyType Type, string? ContactInfo)
    : IRequest<CounterpartyResponse>;

public class CreateCounterpartyCommandHandler(ICounterpartyRepository repository, ICurrentUserService currentUserService)
    : IRequestHandler<CreateCounterpartyCommand, CounterpartyResponse>
{
    public async Task<CounterpartyResponse> Handle(CreateCounterpartyCommand request, CancellationToken cancellationToken)
    {
        var counterparty = Counterparty.Create(request.Name, request.Type, request.ContactInfo, currentUserService.UserId!.Value);
        await repository.AddAsync(counterparty, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return counterparty.ToResponse();
    }
}

public class CreateCounterpartyCommandValidator : AbstractValidator<CreateCounterpartyCommand>
{
    public CreateCounterpartyCommandValidator()
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

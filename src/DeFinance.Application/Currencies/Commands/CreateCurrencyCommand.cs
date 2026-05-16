using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Currency;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Currencies.Commands;

public record CreateCurrencyCommand(string Code, string Name, string Symbol) : IRequest<CurrencyResponse>;

public class CreateCurrencyCommandHandler(ICurrencyRepository repository)
    : IRequestHandler<CreateCurrencyCommand, CurrencyResponse>
{
    public async Task<CurrencyResponse> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = Domain.Entities.Currency.Create(request.Code, request.Name, request.Symbol);
        await repository.AddAsync(currency, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return currency.ToResponse();
    }
}

public class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Currency code is required.")
            .MaximumLength(10).WithMessage("Currency code must not exceed 10 characters.")
            .Matches("^[A-Za-z]+$").WithMessage("Currency code must contain letters only.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Currency name is required.")
            .MaximumLength(100).WithMessage("Currency name must not exceed 100 characters.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Currency symbol is required.")
            .MaximumLength(10).WithMessage("Currency symbol must not exceed 10 characters.");
    }
}

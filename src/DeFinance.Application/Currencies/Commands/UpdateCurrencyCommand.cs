using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Currency;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Currencies.Commands;

public record UpdateCurrencyCommand(Guid Id, string Name, string Symbol) : IRequest<CurrencyResponse?>;

public class UpdateCurrencyCommandHandler(ICurrencyRepository repository)
    : IRequestHandler<UpdateCurrencyCommand, CurrencyResponse?>
{
    public async Task<CurrencyResponse?> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (currency is null) return null;

        currency.Update(request.Name, request.Symbol);
        await repository.SaveChangesAsync(cancellationToken);
        return currency.ToResponse();
    }
}

public class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Currency name is required.")
            .MaximumLength(100).WithMessage("Currency name must not exceed 100 characters.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Currency symbol is required.")
            .MaximumLength(10).WithMessage("Currency symbol must not exceed 10 characters.");
    }
}

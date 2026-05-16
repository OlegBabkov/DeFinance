using DeFinance.Application.DTOs.Currency;
using FluentValidation;

namespace DeFinance.Application.Validators;

public class CreateCurrencyRequestValidator : AbstractValidator<CreateCurrencyRequest>
{
    public CreateCurrencyRequestValidator()
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

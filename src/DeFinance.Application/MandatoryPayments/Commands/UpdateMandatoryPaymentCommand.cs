using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.MandatoryPayment;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.MandatoryPayments.Commands;

public record UpdateMandatoryPaymentCommand(
    Guid Id,
    string Name,
    decimal Amount,
    Guid CurrencyId,
    Guid AccountId,
    Guid? CategoryId,
    PaymentFrequency Frequency,
    int DayOfPeriod,
    string? Notes
) : IRequest<MandatoryPaymentResponse?>;

public class UpdateMandatoryPaymentCommandHandler(IMandatoryPaymentRepository repository)
    : IRequestHandler<UpdateMandatoryPaymentCommand, MandatoryPaymentResponse?>
{
    public async Task<MandatoryPaymentResponse?> Handle(
        UpdateMandatoryPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (payment is null) return null;

        payment.Update(
            request.Name, request.Amount,
            request.CurrencyId, request.AccountId, request.CategoryId,
            request.Frequency, request.DayOfPeriod, request.Notes);

        await repository.SaveChangesAsync(cancellationToken);
        return payment.ToResponse();
    }
}

public class UpdateMandatoryPaymentCommandValidator : AbstractValidator<UpdateMandatoryPaymentCommand>
{
    public UpdateMandatoryPaymentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CurrencyId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Frequency).IsInEnum();
        RuleFor(x => x.DayOfPeriod).InclusiveBetween(1, 31);
    }
}

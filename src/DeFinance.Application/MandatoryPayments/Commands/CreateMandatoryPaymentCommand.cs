using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.MandatoryPayment;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.MandatoryPayments.Commands;

public record CreateMandatoryPaymentCommand(
    string Name,
    decimal Amount,
    Guid CurrencyId,
    Guid AccountId,
    Guid? CategoryId,
    Guid? PaymentStatusId,
    PaymentFrequency Frequency,
    int DayOfPeriod,
    string? Notes
) : IRequest<MandatoryPaymentResponse>;

public class CreateMandatoryPaymentCommandHandler(IMandatoryPaymentRepository repository)
    : IRequestHandler<CreateMandatoryPaymentCommand, MandatoryPaymentResponse>
{
    public async Task<MandatoryPaymentResponse> Handle(
        CreateMandatoryPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = MandatoryPayment.Create(
            request.Name, request.Amount,
            request.CurrencyId, request.AccountId, request.CategoryId, request.PaymentStatusId,
            request.Frequency, request.DayOfPeriod, request.Notes);

        await repository.AddAsync(payment, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return payment.ToResponse();
    }
}

public class CreateMandatoryPaymentCommandValidator : AbstractValidator<CreateMandatoryPaymentCommand>
{
    public CreateMandatoryPaymentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CurrencyId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Frequency).IsInEnum();
        RuleFor(x => x.DayOfPeriod).InclusiveBetween(1, 31);
    }
}

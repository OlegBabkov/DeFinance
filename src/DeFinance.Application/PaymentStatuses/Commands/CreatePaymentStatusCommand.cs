using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.PaymentStatus;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.PaymentStatuses.Commands;

public record CreatePaymentStatusCommand(string Name, string? Description, string? Color = null) : IRequest<PaymentStatusResponse>;

public class CreatePaymentStatusCommandHandler(IPaymentStatusRepository repository)
    : IRequestHandler<CreatePaymentStatusCommand, PaymentStatusResponse>
{
    public async Task<PaymentStatusResponse> Handle(CreatePaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var status = PaymentStatus.Create(request.Name, request.Description, request.Color);
        await repository.AddAsync(status, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return status.ToResponse();
    }
}

public class CreatePaymentStatusCommandValidator : AbstractValidator<CreatePaymentStatusCommand>
{
    public CreatePaymentStatusCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description is not null);
    }
}

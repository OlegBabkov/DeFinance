using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.PaymentStatus;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.PaymentStatuses.Commands;

public record UpdatePaymentStatusCommand(Guid Id, string Name, string? Description) : IRequest<PaymentStatusResponse?>;

public class UpdatePaymentStatusCommandHandler(IPaymentStatusRepository repository)
    : IRequestHandler<UpdatePaymentStatusCommand, PaymentStatusResponse?>
{
    public async Task<PaymentStatusResponse?> Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var status = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (status is null) return null;
        status.Update(request.Name, request.Description);
        await repository.SaveChangesAsync(cancellationToken);
        return status.ToResponse();
    }
}

public class UpdatePaymentStatusCommandValidator : AbstractValidator<UpdatePaymentStatusCommand>
{
    public UpdatePaymentStatusCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description is not null);
    }
}

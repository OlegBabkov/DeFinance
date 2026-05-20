using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Users.Commands;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<bool>;

public class ChangePasswordCommandHandler(IUserRepository repository, IPasswordService passwordService)
    : IRequestHandler<ChangePasswordCommand, bool>
{
    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return false;

        if (!passwordService.Verify(request.CurrentPassword, user.Password))
            return false;

        user.ChangePassword(passwordService.Hash(request.NewPassword));
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).MaximumLength(100);
    }
}

using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.User;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Users.Commands;

public record UpdateUserCommand(
    Guid Id,
    string Username,
    string Email,
    string? PhoneNumber
) : IRequest<UserResponse?>;

public class UpdateUserCommandHandler(IUserRepository repository, IPasswordService passwordService)
    : IRequestHandler<UpdateUserCommand, UserResponse?>
{
    public async Task<UserResponse?> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null) return null;

        var hashedEmail = passwordService.Hash(request.Email);
        var hashedPhone = request.PhoneNumber is not null ? passwordService.Hash(request.PhoneNumber) : null;
        user.Update(request.Username, hashedEmail, hashedPhone);
        await repository.SaveChangesAsync(cancellationToken);
        return user.ToResponse();
    }
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).MaximumLength(50).When(x => x.PhoneNumber is not null);
    }
}

using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.User;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Users.Commands;

public record CreateUserCommand(
    string Username,
    string Password,
    string Email,
    string? PhoneNumber
) : IRequest<UserResponse>;

public class CreateUserCommandHandler(IUserRepository repository, IPasswordService passwordService)
    : IRequestHandler<CreateUserCommand, UserResponse>
{
    public async Task<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existing is not null)
            throw new ValidationException([new FluentValidation.Results.ValidationFailure(
                nameof(request.Username), "Username is already taken.")]);

        var hashed = passwordService.Hash(request.Password);
        var hashedEmail = passwordService.Hash(request.Email);
        var hashedPhone = request.PhoneNumber is not null ? passwordService.Hash(request.PhoneNumber) : null;
        var user = User.Create(request.Username, hashed, hashedEmail, hashedPhone);
        await repository.AddAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return user.ToResponse();
    }
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(256);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).MaximumLength(50).When(x => x.PhoneNumber is not null);
    }
}

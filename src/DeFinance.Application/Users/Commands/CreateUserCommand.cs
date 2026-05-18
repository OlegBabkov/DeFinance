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

public class CreateUserCommandHandler(IUserRepository repository)
    : IRequestHandler<CreateUserCommand, UserResponse>
{
    public async Task<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = User.Create(request.Username, request.Password, request.Email, request.PhoneNumber);
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
        RuleFor(x => x.Password).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).MaximumLength(30).When(x => x.PhoneNumber is not null);
    }
}

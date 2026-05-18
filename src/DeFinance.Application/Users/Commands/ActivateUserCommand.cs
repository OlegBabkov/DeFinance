using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.User;
using MediatR;

namespace DeFinance.Application.Users.Commands;

public record ActivateUserCommand(Guid Id) : IRequest<UserResponse?>;

public class ActivateUserCommandHandler(IUserRepository repository)
    : IRequestHandler<ActivateUserCommand, UserResponse?>
{
    public async Task<UserResponse?> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null) return null;

        user.Activate();
        await repository.SaveChangesAsync(cancellationToken);
        return user.ToResponse();
    }
}

public record DeactivateUserCommand(Guid Id) : IRequest<UserResponse?>;

public class DeactivateUserCommandHandler(IUserRepository repository)
    : IRequestHandler<DeactivateUserCommand, UserResponse?>
{
    public async Task<UserResponse?> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null) return null;

        user.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return user.ToResponse();
    }
}

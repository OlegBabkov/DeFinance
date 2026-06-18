using DeFinance.Application.Abstractions.Repositories;
using MediatR;

namespace DeFinance.Application.Users.Commands;

public record DeleteUserPhotoCommand(Guid UserId) : IRequest<bool>;

public class DeleteUserPhotoCommandHandler(IUserRepository repository)
    : IRequestHandler<DeleteUserPhotoCommand, bool>
{
    public async Task<bool> Handle(DeleteUserPhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return false;

        user.RemovePhoto();
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}

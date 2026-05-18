using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.User;
using MediatR;

namespace DeFinance.Application.Users.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<UserResponse?>;

public class GetUserByIdQueryHandler(IUserRepository repository)
    : IRequestHandler<GetUserByIdQuery, UserResponse?>
{
    public async Task<UserResponse?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(request.Id, cancellationToken);
        return user?.ToResponse();
    }
}

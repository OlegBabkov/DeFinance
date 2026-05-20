using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using MediatR;

namespace DeFinance.Application.Auth.Commands;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponse?>;

public record LoginResponse(string Token, string Username);

public class LoginCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, LoginResponse?>
{
    public async Task<LoginResponse?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !user.IsActive)
            return null;

        if (!passwordService.Verify(request.Password, user.Password))
            return null;

        var token = jwtTokenService.GenerateToken(user.Id, user.Username, user.Email);
        return new LoginResponse(token, user.Username);
    }
}

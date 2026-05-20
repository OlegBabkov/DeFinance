namespace DeFinance.Application.Abstractions;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string username, string email);
}

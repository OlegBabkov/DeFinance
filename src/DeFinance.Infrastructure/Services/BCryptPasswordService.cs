using DeFinance.Application.Abstractions;

namespace DeFinance.Infrastructure.Services;

public class BCryptPasswordService : IPasswordService
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}

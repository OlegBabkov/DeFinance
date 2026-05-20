namespace DeFinance.Application.Abstractions;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

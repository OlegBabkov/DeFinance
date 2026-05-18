using DeFinance.Application.DTOs.Currency;

namespace DeFinance.Application.DTOs.Account;

public static class AccountMappingExtensions
{
    public static AccountResponse ToResponse(this Domain.Entities.Account account) =>
        new(account.Id, account.Name, account.Type, account.Balance, account.CurrencyId,
            account.Currency?.ToResponse(), account.IsActive);

    public static IReadOnlyList<AccountResponse> ToResponse(this IEnumerable<Domain.Entities.Account> accounts) =>
        accounts.Select(a => a.ToResponse()).ToList();
}

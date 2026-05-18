using DeFinance.Application.DTOs.Currency;
using DeFinance.Domain.Entities;

namespace DeFinance.Application.DTOs.Account;

public record AccountResponse(
    Guid Id,
    string Name,
    AccountType Type,
    decimal Balance,
    Guid CurrencyId,
    CurrencyResponse? Currency,
    bool IsActive);

using DeFinance.Domain.Entities;

namespace DeFinance.Application.DTOs.Account;

public record CreateAccountRequest(
    string Name,
    AccountType Type,
    decimal InitialBalance,
    Guid CurrencyId);

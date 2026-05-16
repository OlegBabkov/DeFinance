namespace DeFinance.Application.DTOs.Currency;

public record CurrencyResponse(
    Guid Id,
    string Code,
    string Name,
    string Symbol,
    bool IsActive);

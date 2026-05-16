namespace DeFinance.Application.DTOs.Currency;

public record CreateCurrencyRequest(
    string Code,
    string Name,
    string Symbol);

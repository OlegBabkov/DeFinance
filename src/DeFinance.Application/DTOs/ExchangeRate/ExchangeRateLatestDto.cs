namespace DeFinance.Application.DTOs.ExchangeRate;

public record ExchangeRateLatestDto(
    Guid CurrencyId,
    string CurrencyCode,
    decimal Rate,
    decimal? PreviousRate,
    DateOnly Date);

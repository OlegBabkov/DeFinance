using DeFinance.Domain.Entities;

namespace DeFinance.Application.DTOs.Currency;

public static class CurrencyMappingExtensions
{
    public static CurrencyResponse ToResponse(this Domain.Entities.Currency currency) =>
        new(currency.Id, currency.Code, currency.Name, currency.Symbol, currency.IsActive);

    public static IReadOnlyList<CurrencyResponse> ToResponse(this IEnumerable<Domain.Entities.Currency> currencies) =>
        currencies.Select(c => c.ToResponse()).ToList();

    public static Domain.Entities.Currency ToDomain(this CreateCurrencyRequest request) =>
        Domain.Entities.Currency.Create(request.Code, request.Name, request.Symbol);
}

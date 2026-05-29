using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.DTOs.Currency;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Currencies.Queries;

public record GetAllCurrenciesQuery(
    string? Search = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    SortDirection SortDirection = SortDirection.Asc
) : IRequest<PagedResult<CurrencyResponse>>;

public class GetAllCurrenciesQueryHandler(ICurrencyRepository repository, ICacheService cache)
    : IRequestHandler<GetAllCurrenciesQuery, PagedResult<CurrencyResponse>>
{
    public async Task<PagedResult<CurrencyResponse>> Handle(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
    {
        var key = $"cur:s={request.Search}&a={request.IsActive}&p={request.Page}&ps={request.PageSize}&sb={request.SortBy}&sd={request.SortDirection}";
        var cached = await cache.GetAsync<PagedResult<CurrencyResponse>>(key, cancellationToken);
        if (cached is not null) return cached;

        var (items, totalCount) = await repository.GetAllAsync(
            request.Search, request.IsActive,
            request.SortBy, request.SortDirection,
            request.Page, request.PageSize,
            cancellationToken);

        var result = new PagedResult<CurrencyResponse>(items.ToResponse(), totalCount, request.Page, request.PageSize);
        await cache.SetAsync(key, result, TimeSpan.FromMinutes(10), cancellationToken);
        return result;
    }
}

public class GetAllCurrenciesQueryValidator : AbstractValidator<GetAllCurrenciesQuery>
{
    private static readonly HashSet<string> ValidSortFields =
        new(["name", "code", "symbol"], StringComparer.OrdinalIgnoreCase);

    public GetAllCurrenciesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 500);
        RuleFor(x => x.SortBy)
            .Must(s => ValidSortFields.Contains(s!))
            .WithMessage("SortBy must be one of: name, code, symbol.")
            .When(x => x.SortBy is not null);
    }
}

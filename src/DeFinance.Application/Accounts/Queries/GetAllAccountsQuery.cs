using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.DTOs.Account;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Accounts.Queries;

public record GetAllAccountsQuery(
    string? Search = null,
    bool? IsActive = null,
    AccountType? Type = null,
    Guid? CurrencyId = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    SortDirection SortDirection = SortDirection.Asc
) : IRequest<PagedResult<AccountResponse>>;

public class GetAllAccountsQueryHandler(IAccountRepository repository, ICacheService cache, ICurrentUserService currentUserService)
    : IRequestHandler<GetAllAccountsQuery, PagedResult<AccountResponse>>
{
    public async Task<PagedResult<AccountResponse>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        var key = $"acc:uid={currentUserService.UserId}&s={request.Search}&a={request.IsActive}&t={request.Type}&c={request.CurrencyId}&p={request.Page}&ps={request.PageSize}&sb={request.SortBy}&sd={request.SortDirection}";
        var cached = await cache.GetAsync<PagedResult<AccountResponse>>(key, cancellationToken);
        if (cached is not null) return cached;

        var (items, totalCount) = await repository.GetAllAsync(
            request.Search, request.IsActive,
            request.Type, request.CurrencyId,
            request.SortBy, request.SortDirection,
            request.Page, request.PageSize,
            cancellationToken);

        var result = new PagedResult<AccountResponse>(items.ToResponse(), totalCount, request.Page, request.PageSize);
        await cache.SetAsync(key, result, TimeSpan.FromMinutes(2), cancellationToken);
        return result;
    }
}

public class GetAllAccountsQueryValidator : AbstractValidator<GetAllAccountsQuery>
{
    private static readonly HashSet<string> ValidSortFields =
        new(["name", "type", "balance"], StringComparer.OrdinalIgnoreCase);

    public GetAllAccountsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 500);
        RuleFor(x => x.SortBy)
            .Must(s => ValidSortFields.Contains(s!))
            .WithMessage("SortBy must be one of: name, type, balance.")
            .When(x => x.SortBy is not null);
    }
}

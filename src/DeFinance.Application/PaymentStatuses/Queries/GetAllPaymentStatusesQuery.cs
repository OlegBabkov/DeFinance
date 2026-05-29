using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.DTOs.PaymentStatus;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.PaymentStatuses.Queries;

public record GetAllPaymentStatusesQuery(
    string? Search = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    SortDirection SortDirection = SortDirection.Asc
) : IRequest<PagedResult<PaymentStatusResponse>>;

public class GetAllPaymentStatusesQueryHandler(IPaymentStatusRepository repository, ICacheService cache)
    : IRequestHandler<GetAllPaymentStatusesQuery, PagedResult<PaymentStatusResponse>>
{
    public async Task<PagedResult<PaymentStatusResponse>> Handle(GetAllPaymentStatusesQuery request, CancellationToken cancellationToken)
    {
        var key = $"ps:s={request.Search}&a={request.IsActive}&p={request.Page}&ps={request.PageSize}&sb={request.SortBy}&sd={request.SortDirection}";
        var cached = await cache.GetAsync<PagedResult<PaymentStatusResponse>>(key, cancellationToken);
        if (cached is not null) return cached;

        var (items, totalCount) = await repository.GetAllAsync(
            request.Search, request.IsActive,
            request.SortBy, request.SortDirection,
            request.Page, request.PageSize,
            cancellationToken);

        var result = new PagedResult<PaymentStatusResponse>(items.ToResponse(), totalCount, request.Page, request.PageSize);
        await cache.SetAsync(key, result, TimeSpan.FromMinutes(5), cancellationToken);
        return result;
    }
}

public class GetAllPaymentStatusesQueryValidator : AbstractValidator<GetAllPaymentStatusesQuery>
{
    private static readonly HashSet<string> ValidSortFields =
        new(["name"], StringComparer.OrdinalIgnoreCase);

    public GetAllPaymentStatusesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 500);
        RuleFor(x => x.SortBy)
            .Must(s => ValidSortFields.Contains(s!))
            .WithMessage("SortBy must be: name.")
            .When(x => x.SortBy is not null);
    }
}

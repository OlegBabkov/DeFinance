using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.DTOs.Counterparty;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Counterparties.Queries;

public record GetAllCounterpartiesQuery(
    string? Search = null,
    bool? IsActive = null,
    CounterpartyType? Type = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    SortDirection SortDirection = SortDirection.Asc
) : IRequest<PagedResult<CounterpartyResponse>>;

public class GetAllCounterpartiesQueryHandler(ICounterpartyRepository repository)
    : IRequestHandler<GetAllCounterpartiesQuery, PagedResult<CounterpartyResponse>>
{
    public async Task<PagedResult<CounterpartyResponse>> Handle(GetAllCounterpartiesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetAllAsync(
            request.Search, request.IsActive,
            request.Type,
            request.SortBy, request.SortDirection,
            request.Page, request.PageSize,
            cancellationToken);

        return new PagedResult<CounterpartyResponse>(items.ToResponse(), totalCount, request.Page, request.PageSize);
    }
}

public class GetAllCounterpartiesQueryValidator : AbstractValidator<GetAllCounterpartiesQuery>
{
    private static readonly HashSet<string> ValidSortFields =
        new(["name", "type"], StringComparer.OrdinalIgnoreCase);

    public GetAllCounterpartiesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 500);
        RuleFor(x => x.SortBy)
            .Must(s => ValidSortFields.Contains(s!))
            .WithMessage("SortBy must be one of: name, type.")
            .When(x => x.SortBy is not null);
    }
}

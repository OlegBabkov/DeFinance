using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.DTOs.Category;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Categories.Queries;

public record GetAllCategoriesQuery(
    string? Search = null,
    bool? IsActive = null,
    CategoryType? Type = null,
    CategoryPaymentObligation? PaymentObligation = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    SortDirection SortDirection = SortDirection.Asc
) : IRequest<PagedResult<CategoryResponse>>;

public class GetAllCategoriesQueryHandler(ICategoryRepository repository)
    : IRequestHandler<GetAllCategoriesQuery, PagedResult<CategoryResponse>>
{
    public async Task<PagedResult<CategoryResponse>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetAllAsync(
            request.Search, request.IsActive,
            request.Type, request.PaymentObligation,
            request.SortBy, request.SortDirection,
            request.Page, request.PageSize,
            cancellationToken);

        return new PagedResult<CategoryResponse>(items.ToResponse(), totalCount, request.Page, request.PageSize);
    }
}

public class GetAllCategoriesQueryValidator : AbstractValidator<GetAllCategoriesQuery>
{
    private static readonly HashSet<string> ValidSortFields =
        new(["name", "type"], StringComparer.OrdinalIgnoreCase);

    public GetAllCategoriesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy)
            .Must(s => ValidSortFields.Contains(s!))
            .WithMessage("SortBy must be one of: name, type.")
            .When(x => x.SortBy is not null);
    }
}

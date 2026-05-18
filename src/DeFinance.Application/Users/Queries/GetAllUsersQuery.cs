using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.DTOs.User;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Users.Queries;

public record GetAllUsersQuery(
    string? Search = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    SortDirection SortDirection = SortDirection.Asc
) : IRequest<PagedResult<UserResponse>>;

public class GetAllUsersQueryHandler(IUserRepository repository)
    : IRequestHandler<GetAllUsersQuery, PagedResult<UserResponse>>
{
    public async Task<PagedResult<UserResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetAllAsync(
            request.Search, request.IsActive,
            request.SortBy, request.SortDirection,
            request.Page, request.PageSize,
            cancellationToken);

        return new PagedResult<UserResponse>(items.ToResponse(), totalCount, request.Page, request.PageSize);
    }
}

public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
{
    private static readonly HashSet<string> ValidSortFields =
        new(["username", "email", "createdat"], StringComparer.OrdinalIgnoreCase);

    public GetAllUsersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy)
            .Must(s => ValidSortFields.Contains(s!))
            .WithMessage("SortBy must be one of: username, email, createdAt.")
            .When(x => x.SortBy is not null);
    }
}

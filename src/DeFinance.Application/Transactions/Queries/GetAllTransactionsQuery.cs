using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.DTOs.Transaction;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Transactions.Queries;

public record GetAllTransactionsQuery(
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    Guid? AccountId = null,
    Guid? CategoryId = null,
    Guid? CounterpartyId = null,
    Guid? PaymentStatusId = null,
    Guid? InCurrencyId = null,
    string? Notes = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    SortDirection SortDirection = SortDirection.Desc
) : IRequest<PagedResult<TransactionResponse>>;

public class GetAllTransactionsQueryHandler(ITransactionRepository repository)
    : IRequestHandler<GetAllTransactionsQuery, PagedResult<TransactionResponse>>
{
    public async Task<PagedResult<TransactionResponse>> Handle(GetAllTransactionsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetAllAsync(
            request.DateFrom, request.DateTo,
            request.AccountId, request.CategoryId,
            request.CounterpartyId, request.PaymentStatusId,
            request.InCurrencyId, request.Notes,
            request.SortBy, request.SortDirection,
            request.Page, request.PageSize,
            cancellationToken);

        return new PagedResult<TransactionResponse>(items.ToResponse(), totalCount, request.Page, request.PageSize);
    }
}

public class GetAllTransactionsQueryValidator : AbstractValidator<GetAllTransactionsQuery>
{
    private static readonly HashSet<string> ValidSortFields =
        new(["datetime", "sum", "amountincurrency", "exchangerate"], StringComparer.OrdinalIgnoreCase);

    public GetAllTransactionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 500);
        RuleFor(x => x.SortBy)
            .Must(s => ValidSortFields.Contains(s!))
            .WithMessage("SortBy must be one of: datetime, sum, amountInCurrency, exchangeRate.")
            .When(x => x.SortBy is not null);
        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom!.Value)
            .WithMessage("DateTo must be greater than or equal to DateFrom.")
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);
    }
}

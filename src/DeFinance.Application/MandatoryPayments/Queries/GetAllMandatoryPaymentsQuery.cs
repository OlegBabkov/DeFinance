using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.DTOs.MandatoryPayment;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.MandatoryPayments.Queries;

public record GetAllMandatoryPaymentsQuery(
    string? Search = null,
    bool? IsActive = null,
    Guid? CurrencyId = null,
    Guid? AccountId = null,
    Guid? CategoryId = null,
    PaymentFrequency? Frequency = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    SortDirection SortDirection = SortDirection.Asc
) : IRequest<PagedResult<MandatoryPaymentResponse>>;

public class GetAllMandatoryPaymentsQueryHandler(IMandatoryPaymentRepository repository)
    : IRequestHandler<GetAllMandatoryPaymentsQuery, PagedResult<MandatoryPaymentResponse>>
{
    public async Task<PagedResult<MandatoryPaymentResponse>> Handle(
        GetAllMandatoryPaymentsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetAllAsync(
            request.Search, request.IsActive,
            request.CurrencyId, request.AccountId, request.CategoryId, request.Frequency,
            request.SortBy, request.SortDirection,
            request.Page, request.PageSize,
            cancellationToken);

        return new PagedResult<MandatoryPaymentResponse>(
            items.ToResponse(), totalCount, request.Page, request.PageSize);
    }
}

public class GetAllMandatoryPaymentsQueryValidator : AbstractValidator<GetAllMandatoryPaymentsQuery>
{
    private static readonly HashSet<string> ValidSortFields =
        new(["name", "amount"], StringComparer.OrdinalIgnoreCase);

    public GetAllMandatoryPaymentsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy)
            .Must(s => ValidSortFields.Contains(s!))
            .WithMessage("SortBy must be one of: name, amount.")
            .When(x => x.SortBy is not null);
    }
}

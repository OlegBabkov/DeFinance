using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.PlanFact;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.PlanFact.Queries;

public record GetPlanFactSummaryQuery(
    int Year,
    IReadOnlyList<int> Months
) : IRequest<PlanFactSummaryResponse>;

public class GetPlanFactSummaryQueryHandler(
    ICategoryRepository categoryRepository,
    IBudgetEntryRepository budgetEntryRepository,
    ITransactionRepository transactionRepository)
    : IRequestHandler<GetPlanFactSummaryQuery, PlanFactSummaryResponse>
{
    public async Task<PlanFactSummaryResponse> Handle(GetPlanFactSummaryQuery request, CancellationToken cancellationToken)
    {
        var months = request.Months.Distinct().OrderBy(m => m).ToList();

        var categories = await categoryRepository.GetActiveByTypesAsync(
            [CategoryType.Income, CategoryType.TransferIn, CategoryType.Expense, CategoryType.TransferOut],
            cancellationToken);

        var incomeCategories = categories
            .Where(c => c.Type == CategoryType.Income || c.Type == CategoryType.TransferIn)
            .OrderBy(c => c.Name).ToList();
        var expenseCategories = categories
            .Where(c => c.Type == CategoryType.Expense || c.Type == CategoryType.TransferOut)
            .OrderBy(c => c.Name).ToList();

        var budgetEntries = await budgetEntryRepository.GetByPeriodAsync(request.Year, months, cancellationToken);
        var transactionTotals = await transactionRepository.GetCategoryMonthlyTotalsAsync(request.Year, months, cancellationToken);

        var monthDataList = new List<PlanFactMonthData>();

        foreach (var month in months)
        {
            var monthStart = DateTime.SpecifyKind(new DateTime(request.Year, month, 1), DateTimeKind.Utc);
            var openingBalance = await transactionRepository.GetSignedBalanceBeforeAsync(monthStart, cancellationToken);

            var entryByCategory = budgetEntries
                .Where(e => e.Month == month)
                .ToDictionary(e => e.CategoryId);

            var factByCategory = transactionTotals
                .Where(t => t.Month == month)
                .ToDictionary(t => t.CategoryId, t => t.Total);

            var incomeRows = incomeCategories.Select(c =>
            {
                var entry = entryByCategory.GetValueOrDefault(c.Id);
                var lines = entry?.Lines.OrderBy(l => l.Order)
                    .Select(l => new PlanFactLineRow(l.Name, l.Amount)).ToList()
                    ?? (IReadOnlyList<PlanFactLineRow>)[];
                return new PlanFactCategoryRow(
                    c.Id, c.Name,
                    entry?.PlannedAmount ?? 0m,
                    factByCategory.TryGetValue(c.Id, out var f) ? f : 0m,
                    lines);
            }).ToList();

            var expenseRows = expenseCategories.Select(c =>
            {
                var entry = entryByCategory.GetValueOrDefault(c.Id);
                var lines = entry?.Lines.OrderBy(l => l.Order)
                    .Select(l => new PlanFactLineRow(l.Name, l.Amount)).ToList()
                    ?? (IReadOnlyList<PlanFactLineRow>)[];
                return new PlanFactCategoryRow(
                    c.Id, c.Name,
                    entry?.PlannedAmount ?? 0m,
                    factByCategory.TryGetValue(c.Id, out var f2) ? f2 : 0m,
                    lines);
            }).ToList();

            monthDataList.Add(new PlanFactMonthData(request.Year, month, openingBalance, incomeRows, expenseRows));
        }

        return new PlanFactSummaryResponse(monthDataList);
    }
}

public class GetPlanFactSummaryQueryValidator : AbstractValidator<GetPlanFactSummaryQuery>
{
    public GetPlanFactSummaryQueryValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Months).NotEmpty().WithMessage("At least one month is required.");
        RuleForEach(x => x.Months).InclusiveBetween(1, 12);
    }
}

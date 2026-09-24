using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.PlanFact;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.PlanFact.Queries;

public record GetPlanFactSummaryQuery(
    int Year,
    IReadOnlyList<int> Months,
    bool ExcludeSavings = false
) : IRequest<PlanFactSummaryResponse>;

public class GetPlanFactSummaryQueryHandler(
    ICategoryRepository categoryRepository,
    IBudgetEntryRepository budgetEntryRepository,
    ITransactionRepository transactionRepository,
    IOpeningBalanceOverrideRepository openingBalanceOverrideRepository)
    : IRequestHandler<GetPlanFactSummaryQuery, PlanFactSummaryResponse>
{
    public async Task<PlanFactSummaryResponse> Handle(GetPlanFactSummaryQuery request, CancellationToken cancellationToken)
    {
        var months = request.Months.Distinct().OrderBy(m => m).ToList();

        var categories = await categoryRepository.GetActiveByTypesAsync(
            [CategoryType.Income, CategoryType.TransferIn, CategoryType.Expense, CategoryType.TransferOut],
            cancellationToken);

        var incomeCategories = categories
            .Where(c => c.Type == CategoryType.Income)
            .OrderByDescending(c => c.IsImportant).ThenBy(c => c.Name).ToList();
        var expenseCategories = categories
            .Where(c => c.Type == CategoryType.Expense)
            .OrderByDescending(c => c.IsImportant).ThenBy(c => c.Name).ToList();
        var transferInCategories = categories
            .Where(c => c.Type == CategoryType.TransferIn)
            .OrderBy(c => c.Name).ToList();
        var transferOutCategories = categories
            .Where(c => c.Type == CategoryType.TransferOut)
            .OrderBy(c => c.Name).ToList();

        var budgetEntries = await budgetEntryRepository.GetByPeriodAsync(request.Year, months, cancellationToken);
        var transactionTotals = await transactionRepository.GetCategoryMonthlyTotalsAsync(request.Year, months, request.ExcludeSavings, cancellationToken);
        // Load ALL overrides for the year so months without an override can cascade from a prior month's override
        var openingOverrides = await openingBalanceOverrideRepository.GetAllByYearAsync(request.Year, cancellationToken);
        var overrideByMonth = openingOverrides.ToDictionary(o => o.Month);

        var monthDataList = new List<PlanFactMonthData>();

        foreach (var month in months)
        {
            decimal openingBalance;
            bool openingIsOverride;
            decimal? planOpeningBalance;
            bool planOpeningIsOverride;

            var monthStart = DateTime.SpecifyKind(new DateTime(request.Year, month, 1), DateTimeKind.Utc);

            if (overrideByMonth.TryGetValue(month, out var ov))
            {
                // Overrides store a full-portfolio amount; when excluding savings they can't be used
                // as an anchor because we'd be mixing a savings-inclusive base with savings-exclusive flow.
                if (ov.Amount.HasValue && !request.ExcludeSavings)
                {
                    openingBalance = ov.Amount.Value;
                    openingIsOverride = true;
                }
                else
                {
                    openingBalance = await ResolveOpeningBalanceAsync(overrideByMonth, month, monthStart, request.Year, request.ExcludeSavings, transactionRepository, cancellationToken);
                    openingIsOverride = false;
                }
                planOpeningBalance = ov.PlanAmount;
                planOpeningIsOverride = ov.PlanAmount.HasValue;
            }
            else
            {
                openingBalance = await ResolveOpeningBalanceAsync(overrideByMonth, month, monthStart, request.Year, request.ExcludeSavings, transactionRepository, cancellationToken);
                openingIsOverride = false;
                planOpeningBalance = null;
                planOpeningIsOverride = false;
            }

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
                    lines, c.IsImportant);
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
                    lines, c.IsImportant);
            }).ToList();

            var transferInRows = transferInCategories.Select(c => new PlanFactCategoryRow(
                c.Id, c.Name, 0m,
                factByCategory.TryGetValue(c.Id, out var ti) ? ti : 0m,
                [], c.IsImportant)).ToList();

            var transferOutRows = transferOutCategories.Select(c => new PlanFactCategoryRow(
                c.Id, c.Name, 0m,
                factByCategory.TryGetValue(c.Id, out var to) ? to : 0m,
                [], c.IsImportant)).ToList();

            monthDataList.Add(new PlanFactMonthData(request.Year, month, openingBalance, openingIsOverride, planOpeningBalance, planOpeningIsOverride, incomeRows, expenseRows, transferInRows, transferOutRows));
        }

        return new PlanFactSummaryResponse(monthDataList);
    }

    // For months without an explicit override, cascade forward from the nearest prior override.
    // Falls back to a full raw sum only when no prior override exists.
    private static async Task<decimal> ResolveOpeningBalanceAsync(
        Dictionary<int, OpeningBalanceOverride> overrideByMonth,
        int month,
        DateTime monthStart,
        int year,
        bool excludeSavings,
        ITransactionRepository transactionRepository,
        CancellationToken cancellationToken)
    {
        // Overrides are full-portfolio anchors. Mixing one with savings-excluded net flow
        // would leave the savings portion of the override permanently baked in, so when
        // savings are excluded we always compute the opening balance from raw transactions.
        if (excludeSavings)
            return await transactionRepository.GetSignedBalanceBeforeAsync(monthStart, true, cancellationToken);

        var priorOverride = overrideByMonth.Values
            .Where(o => o.Month < month && o.Amount.HasValue)
            .MaxBy(o => o.Month);

        if (priorOverride?.Amount is null)
            return await transactionRepository.GetSignedBalanceBeforeAsync(monthStart, excludeSavings, cancellationToken);

        var priorMonthStart = DateTime.SpecifyKind(new DateTime(year, priorOverride.Month, 1), DateTimeKind.Utc);
        var netSincePriorOverride = await transactionRepository.GetSignedBalanceInRangeAsync(
            priorMonthStart, monthStart, excludeSavings, cancellationToken);
        return priorOverride.Amount.Value + netSincePriorOverride;
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

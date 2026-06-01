namespace DeFinance.Application.DTOs.PlanFact;

public record PlanFactLineRow(string Name, decimal Amount);

public record PlanFactCategoryRow(
    Guid CategoryId,
    string CategoryName,
    decimal Plan,
    decimal Fact,
    IReadOnlyList<PlanFactLineRow> Lines,
    bool IsImportant
);

public record PlanFactMonthData(
    int Year,
    int Month,
    decimal OpeningBalance,
    bool OpeningBalanceIsOverride,
    IReadOnlyList<PlanFactCategoryRow> IncomeCategories,
    IReadOnlyList<PlanFactCategoryRow> ExpenseCategories
);

public record PlanFactSummaryResponse(
    IReadOnlyList<PlanFactMonthData> Months
);

namespace DeFinance.Application.DTOs.PlanFact;

public record PlanFactCategoryRow(
    Guid CategoryId,
    string CategoryName,
    decimal Plan,
    decimal Fact
);

public record PlanFactMonthData(
    int Year,
    int Month,
    decimal OpeningBalance,
    IReadOnlyList<PlanFactCategoryRow> IncomeCategories,
    IReadOnlyList<PlanFactCategoryRow> ExpenseCategories
);

public record PlanFactSummaryResponse(
    IReadOnlyList<PlanFactMonthData> Months
);

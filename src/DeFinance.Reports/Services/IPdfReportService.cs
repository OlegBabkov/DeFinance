namespace DeFinance.Reports.Services;

public interface IPdfReportService
{
    Task<byte[]> GenerateCashFlowAsync(Guid userId, DateTime from, DateTime to, Guid? accountId);
    Task<byte[]> GenerateExpenseCategoryBreakdownAsync(Guid userId, DateTime from, DateTime to, Guid[] categoryIds);
    Task<byte[]> GenerateAccountBalanceSummaryAsync(Guid userId, DateTime from, DateTime to);
    Task<byte[]> GenerateCounterpartySpendingAsync(Guid userId, DateTime from, DateTime to, Guid[] counterpartyIds);
}

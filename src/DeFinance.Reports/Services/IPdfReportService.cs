namespace DeFinance.Reports.Services;

public interface IPdfReportService
{
    Task<byte[]> GenerateCashFlowAsync(Guid userId, DateTime from, DateTime to, Guid? accountId);
    Task<byte[]> GenerateExpenseCategoryBreakdownAsync(Guid userId, DateTime from, DateTime to, Guid? categoryId);
    Task<byte[]> GenerateAccountBalanceSummaryAsync(Guid userId, DateTime from, DateTime to);
}

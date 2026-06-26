using DeFinance.Contracts.Messages;
using DeFinance.Domain.Entities;
using DeFinance.Infrastructure.Persistence;
using DeFinance.Reports.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace DeFinance.Reports.Consumers;

public class GenerateReportConsumer(
    DeFinanceDbContext db,
    IPdfReportService pdfService,
    IConnectionMultiplexer redis,
    ILogger<GenerateReportConsumer> logger) : IConsumer<GenerateReportMessage>
{
    public async Task Consume(ConsumeContext<GenerateReportMessage> context)
    {
        var msg = context.Message;
        logger.LogInformation("Processing report {ReportId} of type {Type}", msg.ReportId, msg.ReportType);

        var report = await db.Reports.FirstOrDefaultAsync(r => r.Id == msg.ReportId);
        if (report is null)
        {
            logger.LogWarning("Report {ReportId} not found", msg.ReportId);
            return;
        }

        report.MarkProcessing();
        await db.SaveChangesAsync();

        try
        {
            if (!Enum.TryParse<ReportType>(msg.ReportType, out var reportType))
                throw new InvalidOperationException($"Unknown report type: {msg.ReportType}");

            if (!Enum.TryParse<ReportPeriod>(msg.Period, out var period))
                throw new InvalidOperationException($"Unknown period: {msg.Period}");

            var (from, to) = GetDateRange(period);

            var pdfBytes = reportType switch
            {
                ReportType.CashFlowStatement        => await pdfService.GenerateCashFlowAsync(msg.UserId, from, to, msg.AccountId),
                ReportType.ExpenseCategoryBreakdown => await pdfService.GenerateExpenseCategoryBreakdownAsync(msg.UserId, from, to, msg.CategoryIds),
                ReportType.AccountBalanceSummary    => await pdfService.GenerateAccountBalanceSummaryAsync(msg.UserId, from, to),
                ReportType.CounterpartySpending     => await pdfService.GenerateCounterpartySpendingAsync(msg.UserId, from, to, msg.CounterpartyIds),
                _ => throw new InvalidOperationException($"Unhandled report type: {reportType}")
            };

            var fileName = $"{reportType}_{period}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            report.MarkCompleted(pdfBytes, fileName);
            await db.SaveChangesAsync();

            await PublishCompletionAsync(msg.ReportId, msg.UserId, true);
            logger.LogInformation("Report {ReportId} completed ({Bytes} bytes)", msg.ReportId, pdfBytes.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Report {ReportId} failed", msg.ReportId);
            report.MarkFailed(ex.Message);
            await db.SaveChangesAsync();
            await PublishCompletionAsync(msg.ReportId, msg.UserId, false, ex.Message);
        }
    }

    private async Task PublishCompletionAsync(Guid reportId, Guid userId, bool success, string? error = null)
    {
        var payload = System.Text.Json.JsonSerializer.Serialize(
            new ReportGeneratedMessage(reportId, userId, success, error));
        await redis.GetSubscriber().PublishAsync(
            RedisChannel.Literal("reports:generated"), payload);
    }

    private static (DateTime From, DateTime To) GetDateRange(ReportPeriod period)
    {
        var to = DateTime.UtcNow;
        var from = period switch
        {
            ReportPeriod.OneDay       => to.AddDays(-1),
            ReportPeriod.LastWeek     => to.AddDays(-7),
            ReportPeriod.LastMonth    => to.AddMonths(-1),
            ReportPeriod.LastTwoMonths => to.AddMonths(-2),
            ReportPeriod.LastHalfYear => to.AddMonths(-6),
            ReportPeriod.LastYear     => to.AddYears(-1),
            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };
        return (from, to);
    }
}

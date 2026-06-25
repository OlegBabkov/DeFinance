namespace DeFinance.Contracts.Messages;

public record GenerateReportMessage(
    Guid ReportId,
    Guid UserId,
    string ReportType,
    string Period,
    Guid? AccountId,
    Guid[] CategoryIds
);

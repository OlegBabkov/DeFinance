namespace DeFinance.Contracts.Messages;

public record ReportGeneratedMessage(
    Guid ReportId,
    Guid UserId,
    bool Success,
    string? ErrorMessage = null
);

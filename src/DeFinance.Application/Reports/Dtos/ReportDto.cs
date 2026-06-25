using DeFinance.Domain.Entities;

namespace DeFinance.Application.Reports.Dtos;

public record ReportDto(
    Guid Id,
    ReportType Type,
    ReportPeriod Period,
    ReportStatus Status,
    Guid? AccountId,
    Guid? CategoryId,
    string? FileName,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

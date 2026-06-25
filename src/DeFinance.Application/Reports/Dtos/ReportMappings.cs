using DeFinance.Domain.Entities;

namespace DeFinance.Application.Reports.Dtos;

public static class ReportMappings
{
    public static ReportDto ToDto(this Report r) => new(
        r.Id, r.Type, r.Period, r.Status,
        r.AccountId, r.CategoryIds.ToArray(),
        r.FileName, r.ErrorMessage,
        r.CreatedAt, r.CompletedAt);
}

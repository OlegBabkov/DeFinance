using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Reports.Dtos;
using MediatR;

namespace DeFinance.Application.Reports.Queries;

public record GetReportsQuery : IRequest<IReadOnlyList<ReportDto>>;

public class GetReportsQueryHandler(
    IReportRepository reportRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetReportsQuery, IReadOnlyList<ReportDto>>
{
    public async Task<IReadOnlyList<ReportDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await reportRepository.GetByUserAsync(currentUserService.UserId, cancellationToken);
        return reports.Select(r => r.ToDto()).ToList();
    }
}

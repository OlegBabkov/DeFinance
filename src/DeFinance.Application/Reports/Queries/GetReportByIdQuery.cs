using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Reports.Dtos;
using MediatR;

namespace DeFinance.Application.Reports.Queries;

public record GetReportByIdQuery(Guid Id) : IRequest<ReportDto?>;

public class GetReportByIdQueryHandler(IReportRepository reportRepository, ICurrentUserService currentUser)
    : IRequestHandler<GetReportByIdQuery, ReportDto?>
{
    public async Task<ReportDto?> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        var report = await reportRepository.GetByIdAsync(request.Id, currentUser.UserId, cancellationToken);
        return report?.ToDto();
    }
}

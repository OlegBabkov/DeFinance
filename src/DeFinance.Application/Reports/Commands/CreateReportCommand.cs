using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Reports.Dtos;
using DeFinance.Domain.Entities;
using MediatR;

namespace DeFinance.Application.Reports.Commands;

public record CreateReportCommand(
    ReportType Type,
    ReportPeriod Period,
    Guid? AccountId,
    Guid[]? CategoryIds,
    Guid[]? CounterpartyIds
) : IRequest<ReportDto>;

public class CreateReportCommandHandler(
    IReportRepository reportRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateReportCommand, ReportDto>
{
    public async Task<ReportDto> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        var report = Report.Create(
            request.Type,
            request.Period,
            currentUserService.UserId,
            request.AccountId,
            request.CategoryIds,
            request.CounterpartyIds);

        await reportRepository.AddAsync(report, cancellationToken);
        await reportRepository.SaveChangesAsync(cancellationToken);

        return report.ToDto();
    }
}

using DeFinance.Application.Abstractions.Repositories;
using MediatR;

namespace DeFinance.Application.Reports.Queries;

public record DownloadReportQuery(Guid Id) : IRequest<(byte[] Content, string FileName)?>;

public class DownloadReportQueryHandler(IReportRepository reportRepository)
    : IRequestHandler<DownloadReportQuery, (byte[] Content, string FileName)?>
{
    public async Task<(byte[] Content, string FileName)?> Handle(DownloadReportQuery request, CancellationToken cancellationToken)
    {
        var report = await reportRepository.GetByIdAsync(request.Id, cancellationToken);
        if (report?.PdfContent is null || report.FileName is null) return null;
        return (report.PdfContent, report.FileName);
    }
}

namespace DeFinance.Domain.Entities;

public class Report
{
    public Guid Id { get; private set; }
    public ReportType Type { get; private set; }
    public ReportPeriod Period { get; private set; }
    public ReportStatus Status { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? AccountId { get; private set; }
    public List<Guid> CategoryIds { get; private set; } = new();
    public byte[]? PdfContent { get; private set; }
    public string? FileName { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private Report() { }

    public static Report Create(ReportType type, ReportPeriod period, Guid userId, Guid? accountId = null, IEnumerable<Guid>? categoryIds = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = type,
            Period = period,
            Status = ReportStatus.Pending,
            UserId = userId,
            AccountId = accountId,
            CategoryIds = categoryIds?.ToList() ?? new(),
            CreatedAt = DateTime.UtcNow
        };

    public void MarkProcessing() => Status = ReportStatus.Processing;

    public void MarkCompleted(byte[] pdfContent, string fileName)
    {
        Status = ReportStatus.Completed;
        PdfContent = pdfContent;
        FileName = fileName;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = ReportStatus.Failed;
        ErrorMessage = error;
        CompletedAt = DateTime.UtcNow;
    }
}

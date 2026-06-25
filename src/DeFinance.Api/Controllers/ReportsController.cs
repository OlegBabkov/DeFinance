using DeFinance.Application.Abstractions;
using DeFinance.Application.Reports.Commands;
using DeFinance.Application.Reports.Queries;
using DeFinance.Contracts.Messages;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeFinance.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(ISender sender, IPublishEndpoint bus, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Generate([FromBody] CreateReportCommand command, CancellationToken ct)
    {
        var report = await sender.Send(command, ct);

        await bus.Publish(new GenerateReportMessage(
            report.Id,
            currentUser.UserId,
            report.Type.ToString(),
            report.Period.ToString(),
            report.AccountId,
            report.CategoryId), ct);

        return Accepted(report);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await sender.Send(new GetReportsQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetReportByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DownloadReportQuery(id), ct);
        if (result is null) return NotFound();
        return File(result.Value.Content, "application/pdf", result.Value.FileName);
    }
}

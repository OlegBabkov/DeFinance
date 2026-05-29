using DeFinance.Application.PlanFact.Commands;
using DeFinance.Application.PlanFact.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeFinance.Api.Controllers;

[ApiController]
[Route("api/plan-fact")]
public class PlanFactController(ISender sender) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] int year,
        [FromQuery] int[] months,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetPlanFactSummaryQuery(year, months), ct));

    [HttpPut("entry")]
    public async Task<IActionResult> UpsertEntry(
        [FromBody] UpsertBudgetEntryCommand command,
        CancellationToken ct = default)
    {
        await sender.Send(command, ct);
        return NoContent();
    }
}

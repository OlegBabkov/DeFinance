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
        [FromQuery] bool excludeSavings = false,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetPlanFactSummaryQuery(year, months, excludeSavings), ct));

    [HttpPut("entry")]
    public async Task<IActionResult> UpsertEntry(
        [FromBody] UpsertBudgetEntryCommand command,
        CancellationToken ct = default)
    {
        await sender.Send(command, ct);
        return NoContent();
    }

    [HttpPut("opening-balance")]
    public async Task<IActionResult> UpsertOpeningBalance(
        [FromBody] UpsertOpeningBalanceCommand command,
        CancellationToken ct = default)
    {
        await sender.Send(command, ct);
        return NoContent();
    }

    [HttpPut("plan-opening-balance")]
    public async Task<IActionResult> UpsertPlanOpeningBalance(
        [FromBody] UpsertPlanOpeningBalanceCommand command,
        CancellationToken ct = default)
    {
        await sender.Send(command, ct);
        return NoContent();
    }
}

using DeFinance.Application.CalendarEvents.Commands;
using DeFinance.Application.CalendarEvents.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeFinance.Api.Controllers;

[ApiController]
[Route("api/calendar-events")]
public class CalendarEventsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? date = null,
        [FromQuery] string? dateFrom = null,
        [FromQuery] string? dateTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 500,
        CancellationToken ct = default)
    {
        if (date is not null)
        {
            if (!DateOnly.TryParse(date, out var d)) return BadRequest("Invalid date format.");
            return Ok(await sender.Send(new GetCalendarEventsByDateRangeQuery(d, d, page, pageSize), ct));
        }
        if (dateFrom is not null && dateTo is not null)
        {
            if (!DateOnly.TryParse(dateFrom, out var from) || !DateOnly.TryParse(dateTo, out var to))
                return BadRequest("Invalid date format.");
            return Ok(await sender.Send(new GetCalendarEventsByDateRangeQuery(from, to, page, pageSize), ct));
        }
        return BadRequest("Provide either 'date' or 'dateFrom'+'dateTo'.");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCalendarEventCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { date = result.Date }, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await sender.Send(new DeleteCalendarEventCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

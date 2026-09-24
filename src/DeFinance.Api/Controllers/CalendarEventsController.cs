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
    public async Task<IActionResult> GetByDate([FromQuery] string date, [FromQuery] int page = 1, [FromQuery] int pageSize = 200, CancellationToken ct = default)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");
        return Ok(await sender.Send(new GetCalendarEventsByDateQuery(parsedDate, page, pageSize), ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCalendarEventCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetByDate), new { date = result.Date }, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await sender.Send(new DeleteCalendarEventCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

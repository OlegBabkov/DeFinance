using DeFinance.Application.Common;
using DeFinance.Application.Transactions.Commands;
using DeFinance.Application.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeFinance.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? counterpartyId,
        [FromQuery] Guid? paymentStatusId,
        [FromQuery] Guid? inCurrencyId,
        [FromQuery] string? notes,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] SortDirection sortDirection = SortDirection.Desc,
        CancellationToken ct = default)
    {
        // ASP.NET model binding produces DateTimeKind.Unspecified from date strings; Npgsql
        // requires DateTimeKind.Utc for timestamptz columns.
        var fromUtc = dateFrom.HasValue ? DateTime.SpecifyKind(dateFrom.Value, DateTimeKind.Utc) : (DateTime?)null;
        var toUtc = dateTo.HasValue ? DateTime.SpecifyKind(dateTo.Value.Date.AddDays(1), DateTimeKind.Utc) : (DateTime?)null;
        return Ok(await sender.Send(
            new GetAllTransactionsQuery(fromUtc, toUtc, accountId, categoryId, counterpartyId,
                paymentStatusId, inCurrencyId, notes, page, pageSize, sortBy, sortDirection), ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetTransactionByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await sender.Send(command, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/balance-before")]
    public async Task<IActionResult> GetBalanceBefore(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetBalanceBeforeTransactionQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await sender.Send(new DeleteTransactionCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

using DeFinance.Application.Common;
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
        CancellationToken ct = default) =>
        Ok(await sender.Send(
            new GetAllTransactionsQuery(dateFrom, dateTo, accountId, categoryId, counterpartyId,
                paymentStatusId, inCurrencyId, notes, page, pageSize, sortBy, sortDirection), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetTransactionByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}

using DeFinance.Application.ExchangeRates.Commands;
using DeFinance.Application.ExchangeRates.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeFinance.Api.Controllers;

[ApiController]
[Route("api/exchange-rates")]
public class ExchangeRatesController(ISender sender) : ControllerBase
{
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest(CancellationToken ct) =>
        Ok(await sender.Send(new GetLatestExchangeRatesQuery(), ct));

    [HttpGet("{currencyId:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid currencyId, [FromQuery] int days = 30, CancellationToken ct = default) =>
        Ok(await sender.Send(new GetExchangeRateHistoryQuery(currencyId, days), ct));

    [HttpPost("sync")]
    public async Task<IActionResult> Sync(CancellationToken ct)
    {
        var count = await sender.Send(new SyncExchangeRatesCommand(), ct);
        return Ok(new { synced = count });
    }
}

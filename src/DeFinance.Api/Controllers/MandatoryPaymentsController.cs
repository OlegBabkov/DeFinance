using DeFinance.Application.Common;
using DeFinance.Application.MandatoryPayments.Commands;
using DeFinance.Application.MandatoryPayments.Queries;
using DeFinance.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeFinance.Api.Controllers;

[ApiController]
[Route("api/mandatory-payments")]
public class MandatoryPaymentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] Guid? currencyId,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? paymentStatusId,
        [FromQuery] PaymentFrequency? frequency,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] SortDirection sortDirection = SortDirection.Asc,
        CancellationToken ct = default) =>
        Ok(await sender.Send(
            new GetAllMandatoryPaymentsQuery(
                search, isActive, currencyId, accountId, categoryId, paymentStatusId, frequency,
                page, pageSize, sortBy, sortDirection), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetMandatoryPaymentByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMandatoryPaymentCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMandatoryPaymentCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command with { Id = id }, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new ActivateMandatoryPaymentCommand(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeactivateMandatoryPaymentCommand(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/payment-status")]
    public async Task<IActionResult> UpdatePaymentStatus(Guid id, [FromBody] UpdatePaymentStatusRequest req, CancellationToken ct)
    {
        var ok = await sender.Send(new UpdateMandatoryPaymentStatusCommand(id, req.PaymentStatusId), ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPatch("reset-payment-statuses")]
    public async Task<IActionResult> ResetPaymentStatuses([FromQuery] Guid accountId, CancellationToken ct)
    {
        var count = await sender.Send(new ResetMandatoryPaymentStatusesCommand(accountId), ct);
        return Ok(new { updated = count });
    }
}

public record UpdatePaymentStatusRequest(Guid? PaymentStatusId);

using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Currency;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DeFinance.Api.Controllers;

[ApiController]
[Route("api/currencies")]
public class CurrenciesController(
    ICurrencyRepository repository,
    IValidator<CreateCurrencyRequest> createValidator,
    IValidator<UpdateCurrencyRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var currencies = await repository.GetAllAsync(ct);
        return Ok(currencies.ToResponse());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var currency = await repository.GetByIdAsync(id, ct);
        return currency is null ? NotFound() : Ok(currency.ToResponse());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCurrencyRequest request, CancellationToken ct)
    {
        var result = await createValidator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem(ModelState);
        }

        var currency = request.ToDomain();
        await repository.AddAsync(currency, ct);
        await repository.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = currency.Id }, currency.ToResponse());
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCurrencyRequest request, CancellationToken ct)
    {
        var result = await updateValidator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem(ModelState);
        }

        var currency = await repository.GetByIdAsync(id, ct);
        if (currency is null) return NotFound();

        currency.Update(request.Name, request.Symbol);
        await repository.SaveChangesAsync(ct);

        return Ok(currency.ToResponse());
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var currency = await repository.GetByIdAsync(id, ct);
        if (currency is null) return NotFound();

        currency.Activate();
        await repository.SaveChangesAsync(ct);

        return Ok(currency.ToResponse());
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var currency = await repository.GetByIdAsync(id, ct);
        if (currency is null) return NotFound();

        currency.Deactivate();
        await repository.SaveChangesAsync(ct);

        return Ok(currency.ToResponse());
    }
}

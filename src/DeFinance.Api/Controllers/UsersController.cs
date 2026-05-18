using DeFinance.Application.Common;
using DeFinance.Application.Users.Commands;
using DeFinance.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeFinance.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] SortDirection sortDirection = SortDirection.Asc,
        CancellationToken ct = default) =>
        Ok(await sender.Send(new GetAllUsersQuery(search, isActive, page, pageSize, sortBy, sortDirection), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetUserByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command with { Id = id }, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new ActivateUserCommand(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeactivateUserCommand(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}

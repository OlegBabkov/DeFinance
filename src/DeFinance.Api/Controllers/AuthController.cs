using System.Security.Claims;
using DeFinance.Application.Auth.Commands;
using DeFinance.Application.Users.Commands;
using DeFinance.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeFinance.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ISender sender) : ControllerBase
{
    public record RegisterRequest(string Username, string Password, string ConfirmPassword, string Email);
    public record UpdateMeRequest(string Username, string Email, string? PhoneNumber);
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        if (req.Password != req.ConfirmPassword)
            return BadRequest(new { message = "Passwords do not match." });

        var result = await sender.Send(
            new CreateUserCommand(req.Username, req.Password, req.Email, null), ct);
        return Created(string.Empty, result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand cmd, CancellationToken ct)
    {
        var result = await sender.Send(cmd, ct);
        return result is null
            ? Unauthorized(new { message = "Invalid username or password." })
            : Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        var result = await sender.Send(new GetUserByIdQuery(userId.Value), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateMeRequest req, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        var result = await sender.Send(new UpdateUserCommand(userId.Value, req.Username, req.Email, req.PhoneNumber), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        var success = await sender.Send(new ChangePasswordCommand(userId.Value, req.CurrentPassword, req.NewPassword), ct);
        return success ? NoContent() : BadRequest(new { message = "Current password is incorrect." });
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

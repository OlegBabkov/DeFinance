using DeFinance.Application.Auth.Commands;
using DeFinance.Application.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeFinance.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController(ISender sender) : ControllerBase
{
    public record RegisterRequest(string Username, string Password, string ConfirmPassword, string Email);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        if (req.Password != req.ConfirmPassword)
            return BadRequest(new { message = "Passwords do not match." });

        var result = await sender.Send(
            new CreateUserCommand(req.Username, req.Password, req.Email, null), ct);
        return Created(string.Empty, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand cmd, CancellationToken ct)
    {
        var result = await sender.Send(cmd, ct);
        return result is null
            ? Unauthorized(new { message = "Invalid username or password." })
            : Ok(result);
    }
}

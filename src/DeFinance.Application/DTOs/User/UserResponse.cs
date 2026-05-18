namespace DeFinance.Application.DTOs.User;

public record UserResponse(
    Guid Id,
    string Username,
    string Email,
    string? PhoneNumber,
    DateTime CreatedAt,
    bool IsActive);

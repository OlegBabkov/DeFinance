namespace DeFinance.Application.DTOs.User;

public record UserResponse(
    Guid Id,
    string Username,
    DateTime CreatedAt,
    bool IsActive);

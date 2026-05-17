namespace DeFinance.Application.DTOs.PaymentStatus;

public record PaymentStatusResponse(Guid Id, string Name, string? Description, bool IsActive);

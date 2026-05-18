using DeFinance.Domain.Entities;

namespace DeFinance.Application.DTOs.Category;

public record CategoryResponse(
    Guid Id,
    string Name,
    CategoryType Type,
    string? Color,
    string? Icon,
    Guid? ParentId,
    CategoryPaymentObligation? PaymentObligation,
    bool IsActive);

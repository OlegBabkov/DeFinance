using DeFinance.Domain.Entities;

namespace DeFinance.Application.DTOs.Category;

public record CreateCategoryRequest(
    string Name,
    CategoryType Type,
    string? Color,
    string? Icon,
    Guid? ParentId);

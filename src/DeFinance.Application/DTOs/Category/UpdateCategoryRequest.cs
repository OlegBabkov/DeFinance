namespace DeFinance.Application.DTOs.Category;

public record UpdateCategoryRequest(
    string Name,
    string? Color,
    string? Icon);

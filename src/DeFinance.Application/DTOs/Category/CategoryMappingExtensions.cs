namespace DeFinance.Application.DTOs.Category;

public static class CategoryMappingExtensions
{
    public static CategoryResponse ToResponse(this Domain.Entities.Category category) =>
        new(category.Id, category.Name, category.Type, category.Color, category.Icon, category.ParentId, category.Parent?.Name, category.PaymentObligation, category.IsActive);

    public static IReadOnlyList<CategoryResponse> ToResponse(this IEnumerable<Domain.Entities.Category> categories) =>
        categories.Select(c => c.ToResponse()).ToList();
}

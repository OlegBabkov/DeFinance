using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Category;
using MediatR;

namespace DeFinance.Application.Categories.Commands;

public record DeactivateCategoryCommand(Guid Id) : IRequest<CategoryResponse?>;

public class DeactivateCategoryCommandHandler(ICategoryRepository repository)
    : IRequestHandler<DeactivateCategoryCommand, CategoryResponse?>
{
    public async Task<CategoryResponse?> Handle(DeactivateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null) return null;

        category.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return category.ToResponse();
    }
}

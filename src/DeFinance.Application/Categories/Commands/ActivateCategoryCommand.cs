using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Category;
using MediatR;

namespace DeFinance.Application.Categories.Commands;

public record ActivateCategoryCommand(Guid Id) : IRequest<CategoryResponse?>;

public class ActivateCategoryCommandHandler(ICategoryRepository repository)
    : IRequestHandler<ActivateCategoryCommand, CategoryResponse?>
{
    public async Task<CategoryResponse?> Handle(ActivateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null) return null;

        category.Activate();
        await repository.SaveChangesAsync(cancellationToken);
        return category.ToResponse();
    }
}

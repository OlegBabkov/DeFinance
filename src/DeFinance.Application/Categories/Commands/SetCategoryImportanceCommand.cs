using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Category;
using MediatR;

namespace DeFinance.Application.Categories.Commands;

public record SetCategoryImportanceCommand(Guid Id, bool IsImportant) : IRequest<CategoryResponse?>;

public class SetCategoryImportanceCommandHandler(ICategoryRepository repository)
    : IRequestHandler<SetCategoryImportanceCommand, CategoryResponse?>
{
    public async Task<CategoryResponse?> Handle(SetCategoryImportanceCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null) return null;

        category.SetImportant(request.IsImportant);
        await repository.SaveChangesAsync(cancellationToken);
        return category.ToResponse();
    }
}

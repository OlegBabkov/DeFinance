using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Category;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Categories.Commands;

public record UpdateCategoryCommand(Guid Id, string Name, string? Color, string? Icon) : IRequest<CategoryResponse?>;

public class UpdateCategoryCommandHandler(ICategoryRepository repository)
    : IRequestHandler<UpdateCategoryCommand, CategoryResponse?>
{
    public async Task<CategoryResponse?> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null) return null;

        category.Update(request.Name, request.Color, request.Icon);
        await repository.SaveChangesAsync(cancellationToken);
        return category.ToResponse();
    }
}

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Color)
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Color must be a valid hex value (e.g. #FF5733).")
            .When(x => x.Color is not null);

        RuleFor(x => x.Icon)
            .MaximumLength(50).WithMessage("Icon must not exceed 50 characters.")
            .When(x => x.Icon is not null);
    }
}

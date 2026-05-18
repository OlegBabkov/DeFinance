using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Category;
using DeFinance.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DeFinance.Application.Categories.Commands;

public record CreateCategoryCommand(string Name, CategoryType Type, string? Color, string? Icon, Guid? ParentId, CategoryPaymentObligation? PaymentObligation)
    : IRequest<CategoryResponse>;

public class CreateCategoryCommandHandler(ICategoryRepository repository)
    : IRequestHandler<CreateCategoryCommand, CategoryResponse>
{
    public async Task<CategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(request.Name, request.Type, request.Color, request.Icon, request.ParentId, request.PaymentObligation);
        await repository.AddAsync(category, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return category.ToResponse();
    }
}

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Category type is invalid.");

        RuleFor(x => x.Color)
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Color must be a valid hex value (e.g. #FF5733).")
            .When(x => x.Color is not null);

        RuleFor(x => x.Icon)
            .MaximumLength(50).WithMessage("Icon must not exceed 50 characters.")
            .When(x => x.Icon is not null);

        RuleFor(x => x.PaymentObligation)
            .IsInEnum().WithMessage("Payment obligation is invalid.")
            .When(x => x.PaymentObligation is not null);
    }
}

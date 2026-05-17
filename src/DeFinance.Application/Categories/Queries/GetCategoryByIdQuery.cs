using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Category;
using MediatR;

namespace DeFinance.Application.Categories.Queries;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryResponse?>;

public class GetCategoryByIdQueryHandler(ICategoryRepository repository)
    : IRequestHandler<GetCategoryByIdQuery, CategoryResponse?>
{
    public async Task<CategoryResponse?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        return category?.ToResponse();
    }
}

using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.Category;
using MediatR;

namespace DeFinance.Application.Categories.Queries;

public record GetAllCategoriesQuery : IRequest<IReadOnlyList<CategoryResponse>>;

public class GetAllCategoriesQueryHandler(ICategoryRepository repository)
    : IRequestHandler<GetAllCategoriesQuery, IReadOnlyList<CategoryResponse>>
{
    public async Task<IReadOnlyList<CategoryResponse>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken) =>
        (await repository.GetAllAsync(cancellationToken)).ToResponse();
}

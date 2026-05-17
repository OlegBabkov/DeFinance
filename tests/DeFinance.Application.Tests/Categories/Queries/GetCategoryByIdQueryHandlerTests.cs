using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Categories.Queries;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Categories.Queries;

public class GetCategoryByIdQueryHandlerTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly GetCategoryByIdQueryHandler _handler;

    public GetCategoryByIdQueryHandlerTests()
    {
        _handler = new GetCategoryByIdQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenCategoryExists_ShouldReturnResponse()
    {
        var id = Guid.NewGuid();
        var category = Category.Create("Transport", CategoryType.Expense, "#0000FF", "🚗", null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(category);

        var result = await _handler.Handle(new GetCategoryByIdQuery(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Transport");
        result.Type.Should().Be(CategoryType.Expense);
        result.Color.Should().Be("#0000FF");
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var result = await _handler.Handle(new GetCategoryByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}

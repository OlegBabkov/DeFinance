using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Categories.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Categories.Commands;

public class CreateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _handler = new CreateCategoryCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldCreateCategoryAndReturnResponse()
    {
        var command = new CreateCategoryCommand("Food", CategoryType.Expense, "#FF5733", "🍔", null, null);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Food");
        result.Type.Should().Be(CategoryType.Expense);
        result.Color.Should().Be("#FF5733");
        result.Icon.Should().Be("🍔");
        result.ParentId.Should().BeNull();
        result.IsActive.Should().BeTrue();

        await _repository.Received(1).AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithParentId_ShouldSetParentId()
    {
        var parentId = Guid.NewGuid();
        var command = new CreateCategoryCommand("Fast Food", CategoryType.Expense, null, null, parentId, null);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ParentId.Should().Be(parentId);
    }

    [Fact]
    public async Task Handle_WithNullColorAndIcon_ShouldCreateSuccessfully()
    {
        var command = new CreateCategoryCommand("Salary", CategoryType.Income, null, null, null, null);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Color.Should().BeNull();
        result.Icon.Should().BeNull();
    }
}

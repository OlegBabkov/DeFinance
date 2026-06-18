using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Categories.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Categories.Commands;

public class ActivateDeactivateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();

    [Fact]
    public async Task Activate_WhenCategoryExists_ShouldSetIsActiveTrue()
    {
        var id = Guid.NewGuid();
        var category = Category.Create("Food", CategoryType.Expense, null, null, null, null, Guid.NewGuid());
        category.Deactivate();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(category);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new ActivateCategoryCommandHandler(_repository)
            .Handle(new ActivateCategoryCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenCategoryNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var result = await new ActivateCategoryCommandHandler(_repository)
            .Handle(new ActivateCategoryCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Deactivate_WhenCategoryExists_ShouldSetIsActiveFalse()
    {
        var id = Guid.NewGuid();
        var category = Category.Create("Salary", CategoryType.Income, null, null, null, null, Guid.NewGuid());
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(category);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new DeactivateCategoryCommandHandler(_repository)
            .Handle(new DeactivateCategoryCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WhenCategoryNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var result = await new DeactivateCategoryCommandHandler(_repository)
            .Handle(new DeactivateCategoryCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}

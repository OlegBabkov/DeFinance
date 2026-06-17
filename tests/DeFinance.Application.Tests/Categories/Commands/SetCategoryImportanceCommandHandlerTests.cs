using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Categories.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Categories.Commands;

public class SetCategoryImportanceCommandHandlerTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly SetCategoryImportanceCommandHandler _handler;

    public SetCategoryImportanceCommandHandlerTests()
    {
        _handler = new SetCategoryImportanceCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenCategoryExists_ShouldSetImportantTrue()
    {
        var id = Guid.NewGuid();
        var category = Category.Create("Salary", CategoryType.Income, null, null, null, null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(category);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new SetCategoryImportanceCommand(id, true), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsImportant.Should().BeTrue();
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSetToFalse_ShouldClearImportantFlag()
    {
        var id = Guid.NewGuid();
        var category = Category.Create("Salary", CategoryType.Income, null, null, null, null);
        category.SetImportant(true);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(category);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new SetCategoryImportanceCommand(id, false), CancellationToken.None);

        result!.IsImportant.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var result = await _handler.Handle(new SetCategoryImportanceCommand(Guid.NewGuid(), true), CancellationToken.None);

        result.Should().BeNull();
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

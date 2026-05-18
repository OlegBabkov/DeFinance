using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Categories.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Categories.Commands;

public class UpdateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _handler = new UpdateCategoryCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenCategoryExists_ShouldUpdateAndReturnResponse()
    {
        var id = Guid.NewGuid();
        var category = Category.Create("Old Name", CategoryType.Expense, "#000000", "icon", null, null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(category);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new UpdateCategoryCommand(id, "New Name", "#FFFFFF", "new-icon", null), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.Color.Should().Be("#FFFFFF");
        result.Icon.Should().Be("new-icon");
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var result = await _handler.Handle(new UpdateCategoryCommand(Guid.NewGuid(), "Name", null, null, null), CancellationToken.None);

        result.Should().BeNull();
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

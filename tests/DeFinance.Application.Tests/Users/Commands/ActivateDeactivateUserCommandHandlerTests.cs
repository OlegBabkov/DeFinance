using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Users.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Users.Commands;

public class ActivateDeactivateUserCommandHandlerTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();

    [Fact]
    public async Task Activate_WhenUserExists_ShouldSetIsActiveTrue()
    {
        var id = Guid.NewGuid();
        var user = User.Create("alice", "pw", "alice@example.com", null);
        user.Deactivate();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new ActivateUserCommandHandler(_repository)
            .Handle(new ActivateUserCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activate_WhenUserNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await new ActivateUserCommandHandler(_repository)
            .Handle(new ActivateUserCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Deactivate_WhenUserExists_ShouldSetIsActiveFalse()
    {
        var id = Guid.NewGuid();
        var user = User.Create("bob", "pw", "bob@example.com", null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new DeactivateUserCommandHandler(_repository)
            .Handle(new DeactivateUserCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deactivate_WhenUserNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await new DeactivateUserCommandHandler(_repository)
            .Handle(new DeactivateUserCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}

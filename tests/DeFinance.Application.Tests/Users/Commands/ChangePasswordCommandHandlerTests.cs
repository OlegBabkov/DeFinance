using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Users.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Users.Commands;

public class ChangePasswordCommandHandlerTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _handler = new ChangePasswordCommandHandler(_repository, _passwordService);
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordMatches_ShouldChangePasswordAndReturnTrue()
    {
        var id = Guid.NewGuid();
        var user = User.Create("alice", "hashed_old", "alice@example.com", null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);
        _passwordService.Verify("oldpass", "hashed_old").Returns(true);
        _passwordService.Hash("newpass").Returns("hashed_new");
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new ChangePasswordCommand(id, "oldpass", "newpass"), CancellationToken.None);

        result.Should().BeTrue();
        user.Password.Should().Be("hashed_new");
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordWrong_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        var user = User.Create("alice", "hashed_old", "alice@example.com", null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);
        _passwordService.Verify("wrongpass", "hashed_old").Returns(false);

        var result = await _handler.Handle(new ChangePasswordCommand(id, "wrongpass", "newpass"), CancellationToken.None);

        result.Should().BeFalse();
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFalse()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(new ChangePasswordCommand(Guid.NewGuid(), "old", "new"), CancellationToken.None);

        result.Should().BeFalse();
    }
}

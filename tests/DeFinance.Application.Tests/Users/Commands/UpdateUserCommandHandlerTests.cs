using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Users.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Users.Commands;

public class UpdateUserCommandHandlerTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _handler = new UpdateUserCommandHandler(_repository, _passwordService);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldUpdateAndReturnResponse()
    {
        var id = Guid.NewGuid();
        var user = User.Create("alice", "pw", "old@example.com", null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);
        _passwordService.Hash("new@example.com").Returns("hashed_new_email");
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new UpdateUserCommand(id, "alice_updated", "new@example.com", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Username.Should().Be("alice_updated");
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var command = new UpdateUserCommand(Guid.NewGuid(), "name", "email@example.com", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeNull();
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPhoneNumber_ShouldHashPhone()
    {
        var id = Guid.NewGuid();
        var user = User.Create("alice", "pw", "old@example.com", null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);
        _passwordService.Hash(Arg.Any<string>()).Returns(x => "hashed_" + x.Arg<string>());
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        await _handler.Handle(new UpdateUserCommand(id, "alice", "alice@example.com", "+49999"), CancellationToken.None);

        _passwordService.Received(1).Hash("+49999");
    }
}

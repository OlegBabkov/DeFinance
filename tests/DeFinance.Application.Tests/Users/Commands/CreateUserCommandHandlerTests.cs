using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Users.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace DeFinance.Application.Tests.Users.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _handler = new CreateUserCommandHandler(_repository, _passwordService);
    }

    [Fact]
    public async Task Handle_WhenUsernameIsAvailable_ShouldCreateAndReturnUser()
    {
        _repository.GetByUsernameAsync("alice", Arg.Any<CancellationToken>()).Returns((User?)null);
        _passwordService.Hash("secret").Returns("hashed_secret");
        _passwordService.Hash("alice@example.com").Returns("hashed_email");
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new CreateUserCommand("alice", "secret", "alice@example.com", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Username.Should().Be("alice");
        result.IsActive.Should().BeTrue();

        await _repository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUsernameAlreadyExists_ShouldThrowValidationException()
    {
        var existing = User.Create("alice", "pw", "alice@example.com", null);
        _repository.GetByUsernameAsync("alice", Arg.Any<CancellationToken>()).Returns(existing);

        var command = new CreateUserCommand("alice", "secret", "alice@example.com", null);

        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithPhoneNumber_ShouldHashPhone()
    {
        _repository.GetByUsernameAsync("bob", Arg.Any<CancellationToken>()).Returns((User?)null);
        _passwordService.Hash(Arg.Any<string>()).Returns(x => "hashed_" + x.Arg<string>());
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new CreateUserCommand("bob", "pass123", "bob@example.com", "+49123456789");

        await _handler.Handle(command, CancellationToken.None);

        _passwordService.Received(1).Hash("+49123456789");
    }
}

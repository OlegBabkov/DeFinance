using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Auth.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Auth.Commands;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_userRepository, _passwordService, _jwtTokenService);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnTokenAndUsername()
    {
        var user = User.Create("johndoe", "hashed_pw", "john@example.com", null);
        _userRepository.GetByUsernameAsync("johndoe", Arg.Any<CancellationToken>()).Returns(user);
        _passwordService.Verify("secret", "hashed_pw").Returns(true);
        _jwtTokenService.GenerateToken(user.Id, "johndoe", user.Email).Returns("jwt-abc");

        var result = await _handler.Handle(new LoginCommand("johndoe", "secret"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Token.Should().Be("jwt-abc");
        result.Username.Should().Be("johndoe");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnNull()
    {
        _userRepository.GetByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(new LoginCommand("unknown", "pw"), CancellationToken.None);

        result.Should().BeNull();
        _jwtTokenService.DidNotReceive().GenerateToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ShouldReturnNull()
    {
        var user = User.Create("johndoe", "hashed_pw", "john@example.com", null);
        user.Deactivate();
        _userRepository.GetByUsernameAsync("johndoe", Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new LoginCommand("johndoe", "secret"), CancellationToken.None);

        result.Should().BeNull();
        _jwtTokenService.DidNotReceive().GenerateToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenPasswordIncorrect_ShouldReturnNull()
    {
        var user = User.Create("johndoe", "hashed_pw", "john@example.com", null);
        _userRepository.GetByUsernameAsync("johndoe", Arg.Any<CancellationToken>()).Returns(user);
        _passwordService.Verify("wrong", "hashed_pw").Returns(false);

        var result = await _handler.Handle(new LoginCommand("johndoe", "wrong"), CancellationToken.None);

        result.Should().BeNull();
        _jwtTokenService.DidNotReceive().GenerateToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
    }
}

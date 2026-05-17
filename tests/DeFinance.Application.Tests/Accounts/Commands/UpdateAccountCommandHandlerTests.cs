using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Accounts.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Accounts.Commands;

public class UpdateAccountCommandHandlerTests
{
    private readonly IAccountRepository _repository = Substitute.For<IAccountRepository>();
    private readonly UpdateAccountCommandHandler _handler;

    public UpdateAccountCommandHandlerTests()
    {
        _handler = new UpdateAccountCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenAccountExists_ShouldUpdateNameAndReturnResponse()
    {
        var id = Guid.NewGuid();
        var account = Account.Create("Old Name", AccountType.Checking, 500m, Guid.NewGuid());
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(account);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new UpdateAccountCommand(id, "New Name"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Account?)null);

        var result = await _handler.Handle(new UpdateAccountCommand(Guid.NewGuid(), "Name"), CancellationToken.None);

        result.Should().BeNull();
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

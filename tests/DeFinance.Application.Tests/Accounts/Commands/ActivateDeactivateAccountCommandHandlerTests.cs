using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Accounts.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Accounts.Commands;

public class ActivateDeactivateAccountCommandHandlerTests
{
    private readonly IAccountRepository _repository = Substitute.For<IAccountRepository>();

    [Fact]
    public async Task Activate_WhenAccountExists_ShouldSetIsActiveTrue()
    {
        var id = Guid.NewGuid();
        var account = Account.Create("Account", AccountType.Savings, 0m, Guid.NewGuid());
        account.Deactivate();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(account);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new ActivateAccountCommandHandler(_repository)
            .Handle(new ActivateAccountCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenAccountNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Account?)null);

        var result = await new ActivateAccountCommandHandler(_repository)
            .Handle(new ActivateAccountCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Deactivate_WhenAccountExists_ShouldSetIsActiveFalse()
    {
        var id = Guid.NewGuid();
        var account = Account.Create("Account", AccountType.Credit, 0m, Guid.NewGuid());
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(account);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new DeactivateAccountCommandHandler(_repository)
            .Handle(new DeactivateAccountCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WhenAccountNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Account?)null);

        var result = await new DeactivateAccountCommandHandler(_repository)
            .Handle(new DeactivateAccountCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}

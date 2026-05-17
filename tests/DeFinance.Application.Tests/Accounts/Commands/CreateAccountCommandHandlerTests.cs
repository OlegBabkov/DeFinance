using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Accounts.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Accounts.Commands;

public class CreateAccountCommandHandlerTests
{
    private readonly IAccountRepository _repository = Substitute.For<IAccountRepository>();
    private readonly CreateAccountCommandHandler _handler;

    public CreateAccountCommandHandlerTests()
    {
        _handler = new CreateAccountCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldCreateAccountAndReturnResponse()
    {
        var currencyId = Guid.NewGuid();
        var command = new CreateAccountCommand("My Savings", AccountType.Savings, 1000m, currencyId);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("My Savings");
        result.Type.Should().Be(AccountType.Savings);
        result.Balance.Should().Be(1000m);
        result.CurrencyId.Should().Be(currencyId);
        result.IsActive.Should().BeTrue();

        await _repository.Received(1).AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAllowZeroInitialBalance()
    {
        var command = new CreateAccountCommand("Cash Wallet", AccountType.Cash, 0m, Guid.NewGuid());
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Balance.Should().Be(0m);
    }
}

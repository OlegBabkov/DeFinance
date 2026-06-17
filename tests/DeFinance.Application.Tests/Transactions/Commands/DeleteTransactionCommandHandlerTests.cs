using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Transactions.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Transactions.Commands;

public class DeleteTransactionCommandHandlerTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly DeleteTransactionCommandHandler _handler;

    public DeleteTransactionCommandHandlerTests()
    {
        _handler = new DeleteTransactionCommandHandler(_transactionRepository);
    }

    private static Transaction MakeTransactionWithNavProps(Account account, Category category, decimal sum = 200m)
    {
        var tx = Transaction.Create(DateTime.UtcNow, sum, 1m, Guid.NewGuid(), account.Id, category.Id, null, Guid.NewGuid());
        typeof(Transaction).GetProperty("Account")!.SetValue(tx, account);
        typeof(Transaction).GetProperty("Category")!.SetValue(tx, category);
        return tx;
    }

    [Fact]
    public async Task Handle_WhenTransactionExists_IncomeCategory_ShouldReverseBalanceAndRemove()
    {
        var account = Account.Create("Checking", AccountType.Checking, 1200m, Guid.NewGuid());
        var category = Category.Create("Salary", CategoryType.Income, null, null, null, null);
        var tx = MakeTransactionWithNavProps(account, category, 200m);

        _transactionRepository.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>()).Returns(tx);
        _transactionRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new DeleteTransactionCommand(tx.Id), CancellationToken.None);

        result.Should().BeTrue();
        account.Balance.Should().Be(1000m);
        _transactionRepository.Received(1).Remove(tx);
        await _transactionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransactionExists_ExpenseCategory_ShouldReverseBalanceAndRemove()
    {
        var account = Account.Create("Checking", AccountType.Checking, 800m, Guid.NewGuid());
        var category = Category.Create("Rent", CategoryType.Expense, null, null, null, null);
        var tx = MakeTransactionWithNavProps(account, category, 300m);

        _transactionRepository.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>()).Returns(tx);
        _transactionRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new DeleteTransactionCommand(tx.Id), CancellationToken.None);

        result.Should().BeTrue();
        account.Balance.Should().Be(1100m);
    }

    [Fact]
    public async Task Handle_WhenTransactionNotFound_ShouldReturnFalse()
    {
        _transactionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var result = await _handler.Handle(new DeleteTransactionCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeFalse();
        _transactionRepository.DidNotReceive().Remove(Arg.Any<Transaction>());
    }
}

using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Transactions.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Transactions.Commands;

public class UpdateTransactionCommandHandlerTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly UpdateTransactionCommandHandler _handler;

    public UpdateTransactionCommandHandlerTests()
    {
        _handler = new UpdateTransactionCommandHandler(_transactionRepository, _accountRepository, _categoryRepository);
    }

    private static Transaction MakeTransactionWithNavProps(Account account, Category category, decimal sum)
    {
        var tx = Transaction.Create(DateTime.UtcNow, sum, 1m, Guid.NewGuid(), account.Id, category.Id, null, Guid.NewGuid());
        typeof(Transaction).GetProperty("Account")!.SetValue(tx, account);
        typeof(Transaction).GetProperty("Category")!.SetValue(tx, category);
        return tx;
    }

    [Fact]
    public async Task Handle_WhenTransactionNotFound_ShouldReturnNull()
    {
        _transactionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var command = new UpdateTransactionCommand(Guid.NewGuid(), DateTime.UtcNow, 100m, 1m, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SameAccountAndCategory_ShouldReverseThenApplyBalance()
    {
        var account = Account.Create("Checking", AccountType.Checking, 1000m, Guid.NewGuid());
        var category = Category.Create("Salary", CategoryType.Income, null, null, null, null);
        var tx = MakeTransactionWithNavProps(account, category, 300m);
        var returnedTx = Transaction.Create(DateTime.UtcNow, 500m, 1m, Guid.NewGuid(), account.Id, category.Id, null, Guid.NewGuid());

        _transactionRepository.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>()).Returns(tx);
        _transactionRepository.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>()).Returns(tx, returnedTx);
        _transactionRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new UpdateTransactionCommand(tx.Id, DateTime.UtcNow, 500m, 1m, tx.InCurrencyId, account.Id, category.Id, null, tx.PaymentStatusId, null);

        await _handler.Handle(command, CancellationToken.None);

        // reverse -300 then apply +500 → net +200 from original 1000
        account.Balance.Should().Be(1200m);
        await _transactionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNewAccountNotFound_ShouldThrow()
    {
        var oldAccount = Account.Create("Old", AccountType.Checking, 500m, Guid.NewGuid());
        var category = Category.Create("Salary", CategoryType.Income, null, null, null, null);
        var tx = MakeTransactionWithNavProps(oldAccount, category, 200m);
        var newAccountId = Guid.NewGuid();

        _transactionRepository.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>()).Returns(tx);
        _accountRepository.GetByIdAsync(newAccountId, Arg.Any<CancellationToken>()).Returns((Account?)null);

        var command = new UpdateTransactionCommand(tx.Id, DateTime.UtcNow, 200m, 1m, tx.InCurrencyId, newAccountId, category.Id, null, tx.PaymentStatusId, null);

        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();
    }
}

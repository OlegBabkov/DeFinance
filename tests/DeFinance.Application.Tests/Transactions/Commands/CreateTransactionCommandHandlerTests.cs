using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Transactions.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Transactions.Commands;

public class CreateTransactionCommandHandlerTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _handler = new CreateTransactionCommandHandler(_transactionRepository, _accountRepository, _categoryRepository, _currentUserService);
    }

    private static Account MakeAccount(decimal balance = 0m) =>
        Account.Create("Checking", AccountType.Checking, balance, Guid.NewGuid(), Guid.NewGuid());

    private static Category MakeCategory(CategoryType type) =>
        Category.Create("Cat", type, null, null, null, null, Guid.NewGuid());

    private static Transaction MakeTransaction(decimal sum = 100m) =>
        Transaction.Create(DateTime.UtcNow, sum, 1m, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task Handle_IncomeCategory_ShouldIncreaseAccountBalance()
    {
        var account = MakeAccount(1000m);
        var category = MakeCategory(CategoryType.Income);
        var returnedTx = MakeTransaction(500m);

        _accountRepository.GetByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);
        _categoryRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        _transactionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(returnedTx);
        _transactionRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new CreateTransactionCommand(DateTime.UtcNow, 500m, 1m, Guid.NewGuid(), account.Id, category.Id, null, Guid.NewGuid(), null);

        await _handler.Handle(command, CancellationToken.None);

        account.Balance.Should().Be(1500m);
        await _transactionRepository.Received(1).AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        await _transactionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExpenseCategory_ShouldDecreaseAccountBalance()
    {
        var account = MakeAccount(1000m);
        var category = MakeCategory(CategoryType.Expense);
        var returnedTx = MakeTransaction(300m);

        _accountRepository.GetByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);
        _categoryRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        _transactionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(returnedTx);
        _transactionRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new CreateTransactionCommand(DateTime.UtcNow, 300m, 1m, Guid.NewGuid(), account.Id, category.Id, null, Guid.NewGuid(), null);

        await _handler.Handle(command, CancellationToken.None);

        account.Balance.Should().Be(700m);
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ShouldThrowInvalidOperationException()
    {
        _accountRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Account?)null);

        var command = new CreateTransactionCommand(DateTime.UtcNow, 100m, 1m, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), null);

        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowInvalidOperationException()
    {
        var account = MakeAccount();
        _accountRepository.GetByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);
        _categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var command = new CreateTransactionCommand(DateTime.UtcNow, 100m, 1m, Guid.NewGuid(), account.Id, Guid.NewGuid(), null, Guid.NewGuid(), null);

        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();
    }
}

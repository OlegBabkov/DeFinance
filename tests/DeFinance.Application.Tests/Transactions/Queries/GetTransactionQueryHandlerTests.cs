using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.Transactions.Queries;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Transactions.Queries;

public class GetTransactionQueryHandlerTests
{
    private readonly ITransactionRepository _repository = Substitute.For<ITransactionRepository>();

    private static Transaction MakeTransaction(decimal sum = 100m) =>
        Transaction.Create(DateTime.UtcNow, sum, 1m, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid());

    // ── GetById ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_WhenTransactionExists_ShouldReturnResponse()
    {
        var id = Guid.NewGuid();
        var tx = MakeTransaction(250m);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(tx);

        var result = await new GetTransactionByIdQueryHandler(_repository)
            .Handle(new GetTransactionByIdQuery(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Sum.Should().Be(250m);
    }

    [Fact]
    public async Task GetById_WhenTransactionNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var result = await new GetTransactionByIdQueryHandler(_repository)
            .Handle(new GetTransactionByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    // ── GetAll ───────────────────────────────────────────────────────────────

    private void SetupGetAll(IReadOnlyList<Transaction> items, int totalCount, decimal totalSum = 0m, decimal totalAmountInCurrency = 0m) =>
        _repository.GetAllAsync(
            Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
            Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<SortDirection>(),
            Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
        .Returns((items, totalCount, totalSum, totalAmountInCurrency));

    [Fact]
    public async Task GetAll_ShouldReturnPagedListWithTotals()
    {
        var txs = new List<Transaction> { MakeTransaction(100m), MakeTransaction(200m) };
        SetupGetAll(txs, 2, totalSum: 300m, totalAmountInCurrency: 300m);

        var result = await new GetAllTransactionsQueryHandler(_repository)
            .Handle(new GetAllTransactionsQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.TotalSum.Should().Be(300m);
        result.TotalAmountInCurrency.Should().Be(300m);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnEmptyResult()
    {
        SetupGetAll([], 0);

        var result = await new GetAllTransactionsQueryHandler(_repository)
            .Handle(new GetAllTransactionsQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    // ── GetBalanceBefore ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetBalanceBefore_WhenTransactionExists_ShouldReturnBalance()
    {
        var txId = Guid.NewGuid();
        _repository.GetBalanceBeforeAsync(txId, Arg.Any<CancellationToken>()).Returns((decimal?)4500m);

        var result = await new GetBalanceBeforeTransactionQueryHandler(_repository)
            .Handle(new GetBalanceBeforeTransactionQuery(txId), CancellationToken.None);

        result.Should().Be(4500m);
    }

    [Fact]
    public async Task GetBalanceBefore_WhenTransactionNotFound_ShouldReturnNull()
    {
        _repository.GetBalanceBeforeAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((decimal?)null);

        var result = await new GetBalanceBeforeTransactionQueryHandler(_repository)
            .Handle(new GetBalanceBeforeTransactionQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}

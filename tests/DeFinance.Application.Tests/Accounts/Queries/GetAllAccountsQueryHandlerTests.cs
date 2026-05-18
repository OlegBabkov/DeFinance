using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Accounts.Queries;
using DeFinance.Application.Common;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Accounts.Queries;

public class GetAllAccountsQueryHandlerTests
{
    private readonly IAccountRepository _repository = Substitute.For<IAccountRepository>();

    private void SetupGetAll(IReadOnlyList<Account> items, int totalCount) =>
        _repository.GetAllAsync(
            Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<AccountType?>(), Arg.Any<Guid?>(),
            Arg.Any<string?>(), Arg.Any<SortDirection>(),
            Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
        .Returns((items, totalCount));

    [Fact]
    public async Task GetAll_ShouldReturnPagedResultWithMappedItems()
    {
        var currencyId = Guid.NewGuid();
        var accounts = new List<Account>
        {
            Account.Create("Checking", AccountType.Checking, 100m, currencyId),
            Account.Create("Savings", AccountType.Savings, 500m, currencyId)
        };
        SetupGetAll(accounts, 2);

        var result = await new GetAllAccountsQueryHandler(_repository)
            .Handle(new GetAllAccountsQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Select(r => r.Name).Should().BeEquivalentTo(["Checking", "Savings"]);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnEmptyPagedResult()
    {
        SetupGetAll([], 0);

        var result = await new GetAllAccountsQueryHandler(_repository)
            .Handle(new GetAllAccountsQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_ShouldPassAllParametersToRepository()
    {
        SetupGetAll([], 0);
        var currencyId = Guid.NewGuid();

        await new GetAllAccountsQueryHandler(_repository)
            .Handle(new GetAllAccountsQuery(
                Search: "savings", IsActive: true, Type: AccountType.Savings, CurrencyId: currencyId,
                Page: 2, PageSize: 25, SortBy: "balance", SortDirection: SortDirection.Desc),
            CancellationToken.None);

        await _repository.Received(1).GetAllAsync(
            "savings", true, AccountType.Savings, currencyId,
            "balance", SortDirection.Desc, 2, 25, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAll_ShouldReflectPageAndPageSizeInResult()
    {
        SetupGetAll([], 100);

        var result = await new GetAllAccountsQueryHandler(_repository)
            .Handle(new GetAllAccountsQuery(Page: 4, PageSize: 25), CancellationToken.None);

        result.Page.Should().Be(4);
        result.PageSize.Should().Be(25);
        result.TotalCount.Should().Be(100);
        result.TotalPages.Should().Be(4);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 10, 10, 1, false, false)]
    [InlineData(1, 10, 11, 2, false, true)]
    [InlineData(2, 10, 11, 2, true, false)]
    public async Task GetAll_ShouldComputeHasNextAndHasPreviousCorrectly(
        int page, int pageSize, int totalCount, int expectedTotalPages,
        bool expectedHasPrev, bool expectedHasNext)
    {
        SetupGetAll([], totalCount);

        var result = await new GetAllAccountsQueryHandler(_repository)
            .Handle(new GetAllAccountsQuery(Page: page, PageSize: pageSize), CancellationToken.None);

        result.TotalPages.Should().Be(expectedTotalPages);
        result.HasPreviousPage.Should().Be(expectedHasPrev);
        result.HasNextPage.Should().Be(expectedHasNext);
    }
}

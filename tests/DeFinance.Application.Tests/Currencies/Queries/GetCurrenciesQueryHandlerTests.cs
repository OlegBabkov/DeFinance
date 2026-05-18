using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.Currencies.Queries;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Currencies.Queries;

public class GetCurrenciesQueryHandlerTests
{
    private readonly ICurrencyRepository _repository = Substitute.For<ICurrencyRepository>();

    private void SetupGetAll(IReadOnlyList<Currency> items, int totalCount) =>
        _repository.GetAllAsync(
            Arg.Any<string?>(), Arg.Any<bool?>(),
            Arg.Any<string?>(), Arg.Any<SortDirection>(),
            Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
        .Returns((items, totalCount));

    [Fact]
    public async Task GetAll_ShouldReturnPagedResultWithMappedItems()
    {
        var currencies = new List<Currency>
        {
            Currency.Create("USD", "US Dollar", "$"),
            Currency.Create("EUR", "Euro", "€")
        };
        SetupGetAll(currencies, 2);

        var result = await new GetAllCurrenciesQueryHandler(_repository)
            .Handle(new GetAllCurrenciesQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Select(r => r.Code).Should().BeEquivalentTo(["USD", "EUR"]);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnEmptyPagedResult()
    {
        SetupGetAll([], 0);

        var result = await new GetAllCurrenciesQueryHandler(_repository)
            .Handle(new GetAllCurrenciesQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_ShouldPassAllParametersToRepository()
    {
        SetupGetAll([], 0);

        await new GetAllCurrenciesQueryHandler(_repository)
            .Handle(new GetAllCurrenciesQuery(
                Search: "usd", IsActive: true, Page: 2, PageSize: 10,
                SortBy: "code", SortDirection: SortDirection.Desc),
            CancellationToken.None);

        await _repository.Received(1).GetAllAsync(
            "usd", true, "code", SortDirection.Desc, 2, 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAll_ShouldReflectPageAndPageSizeInResult()
    {
        SetupGetAll([], 50);

        var result = await new GetAllCurrenciesQueryHandler(_repository)
            .Handle(new GetAllCurrenciesQuery(Page: 3, PageSize: 10), CancellationToken.None);

        result.Page.Should().Be(3);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(50);
        result.TotalPages.Should().Be(5);
    }

    [Theory]
    [InlineData(1, 5, 11, 3, false, true)]
    [InlineData(2, 5, 11, 3, true, true)]
    [InlineData(3, 5, 11, 3, true, false)]
    public async Task GetAll_ShouldComputeHasNextAndHasPreviousCorrectly(
        int page, int pageSize, int totalCount, int expectedTotalPages,
        bool expectedHasPrev, bool expectedHasNext)
    {
        SetupGetAll([], totalCount);

        var result = await new GetAllCurrenciesQueryHandler(_repository)
            .Handle(new GetAllCurrenciesQuery(Page: page, PageSize: pageSize), CancellationToken.None);

        result.TotalPages.Should().Be(expectedTotalPages);
        result.HasPreviousPage.Should().Be(expectedHasPrev);
        result.HasNextPage.Should().Be(expectedHasNext);
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturnResponse()
    {
        var id = Guid.NewGuid();
        var currency = Currency.Create("GBP", "British Pound", "£");
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(currency);

        var result = await new GetCurrencyByIdQueryHandler(_repository)
            .Handle(new GetCurrencyByIdQuery(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Code.Should().Be("GBP");
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Currency?)null);

        var result = await new GetCurrencyByIdQueryHandler(_repository)
            .Handle(new GetCurrencyByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}

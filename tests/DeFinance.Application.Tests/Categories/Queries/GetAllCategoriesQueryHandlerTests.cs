using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Categories.Queries;
using DeFinance.Application.Common;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Categories.Queries;

public class GetAllCategoriesQueryHandlerTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();

    private void SetupGetAll(IReadOnlyList<Category> items, int totalCount) =>
        _repository.GetAllAsync(
            Arg.Any<string?>(), Arg.Any<bool?>(),
            Arg.Any<CategoryType?>(), Arg.Any<CategoryPaymentObligation?>(),
            Arg.Any<string?>(), Arg.Any<SortDirection>(),
            Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
        .Returns((items, totalCount));

    [Fact]
    public async Task GetAll_ShouldReturnPagedResultWithMappedItems()
    {
        var categories = new List<Category>
        {
            Category.Create("Food", CategoryType.Expense, "#FF0000", "🍕", null, null),
            Category.Create("Salary", CategoryType.Income, "#00FF00", "💰", null, null)
        };
        SetupGetAll(categories, 2);

        var result = await new GetAllCategoriesQueryHandler(_repository, _cache)
            .Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Select(r => r.Name).Should().BeEquivalentTo(["Food", "Salary"]);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnEmptyPagedResult()
    {
        SetupGetAll([], 0);

        var result = await new GetAllCategoriesQueryHandler(_repository, _cache)
            .Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_ShouldPassAllParametersToRepository()
    {
        SetupGetAll([], 0);

        await new GetAllCategoriesQueryHandler(_repository, _cache)
            .Handle(new GetAllCategoriesQuery(
                Search: "food", IsActive: true,
                Type: CategoryType.Expense, PaymentObligation: CategoryPaymentObligation.Mandatory,
                Page: 2, PageSize: 50, SortBy: "name", SortDirection: SortDirection.Desc),
            CancellationToken.None);

        await _repository.Received(1).GetAllAsync(
            "food", true, CategoryType.Expense, CategoryPaymentObligation.Mandatory,
            "name", SortDirection.Desc, 2, 50, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(1, 5, 15, 3, false, true)]
    [InlineData(2, 5, 15, 3, true, true)]
    [InlineData(3, 5, 15, 3, true, false)]
    public async Task GetAll_ShouldComputeHasNextAndHasPreviousCorrectly(
        int page, int pageSize, int totalCount, int expectedTotalPages,
        bool expectedHasPrev, bool expectedHasNext)
    {
        SetupGetAll([], totalCount);

        var result = await new GetAllCategoriesQueryHandler(_repository, _cache)
            .Handle(new GetAllCategoriesQuery(Page: page, PageSize: pageSize), CancellationToken.None);

        result.TotalPages.Should().Be(expectedTotalPages);
        result.HasPreviousPage.Should().Be(expectedHasPrev);
        result.HasNextPage.Should().Be(expectedHasNext);
    }
}

using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.Counterparties.Queries;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Counterparties.Queries;

public class GetCounterpartyQueryHandlerTests
{
    private readonly ICounterpartyRepository _repository = Substitute.For<ICounterpartyRepository>();

    private void SetupGetAll(IReadOnlyList<Counterparty> items, int totalCount) =>
        _repository.GetAllAsync(
            Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CounterpartyType?>(),
            Arg.Any<string?>(), Arg.Any<SortDirection>(),
            Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
        .Returns((items, totalCount));

    [Fact]
    public async Task GetAll_ShouldReturnPagedResultWithMappedItems()
    {
        var counterparties = new List<Counterparty>
        {
            Counterparty.Create("Lidl", CounterpartyType.Company, null),
            Counterparty.Create("Maria", CounterpartyType.Person, "maria@email.com")
        };
        SetupGetAll(counterparties, 2);

        var result = await new GetAllCounterpartiesQueryHandler(_repository)
            .Handle(new GetAllCounterpartiesQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Select(r => r.Name).Should().BeEquivalentTo(["Lidl", "Maria"]);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnEmptyPagedResult()
    {
        SetupGetAll([], 0);

        var result = await new GetAllCounterpartiesQueryHandler(_repository)
            .Handle(new GetAllCounterpartiesQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_ShouldPassAllParametersToRepository()
    {
        SetupGetAll([], 0);

        await new GetAllCounterpartiesQueryHandler(_repository)
            .Handle(new GetAllCounterpartiesQuery(
                Search: "lid", IsActive: true, Type: CounterpartyType.Company,
                Page: 2, PageSize: 25, SortBy: "name", SortDirection: SortDirection.Desc),
            CancellationToken.None);

        await _repository.Received(1).GetAllAsync(
            "lid", true, CounterpartyType.Company, "name", SortDirection.Desc, 2, 25,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(1, 10, 25, 3, false, true)]
    [InlineData(2, 10, 25, 3, true, true)]
    [InlineData(3, 10, 25, 3, true, false)]
    public async Task GetAll_ShouldComputeHasNextAndHasPreviousCorrectly(
        int page, int pageSize, int totalCount, int expectedTotalPages,
        bool expectedHasPrev, bool expectedHasNext)
    {
        SetupGetAll([], totalCount);

        var result = await new GetAllCounterpartiesQueryHandler(_repository)
            .Handle(new GetAllCounterpartiesQuery(Page: page, PageSize: pageSize), CancellationToken.None);

        result.TotalPages.Should().Be(expectedTotalPages);
        result.HasPreviousPage.Should().Be(expectedHasPrev);
        result.HasNextPage.Should().Be(expectedHasNext);
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturnResponse()
    {
        var id = Guid.NewGuid();
        var counterparty = Counterparty.Create("Sparkasse", CounterpartyType.Company, null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(counterparty);

        var result = await new GetCounterpartyByIdQueryHandler(_repository)
            .Handle(new GetCounterpartyByIdQuery(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Sparkasse");
        result.Type.Should().Be(CounterpartyType.Company);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Counterparty?)null);

        var result = await new GetCounterpartyByIdQueryHandler(_repository)
            .Handle(new GetCounterpartyByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}

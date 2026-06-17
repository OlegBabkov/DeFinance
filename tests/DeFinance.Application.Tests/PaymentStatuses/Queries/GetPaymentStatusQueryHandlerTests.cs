using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.PaymentStatuses.Queries;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.PaymentStatuses.Queries;

public class GetPaymentStatusQueryHandlerTests
{
    private readonly IPaymentStatusRepository _repository = Substitute.For<IPaymentStatusRepository>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();

    private void SetupGetAll(IReadOnlyList<PaymentStatus> items, int totalCount) =>
        _repository.GetAllAsync(
            Arg.Any<string?>(), Arg.Any<bool?>(),
            Arg.Any<string?>(), Arg.Any<SortDirection>(),
            Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
        .Returns((items, totalCount));

    [Fact]
    public async Task GetAll_ShouldReturnPagedResultWithMappedItems()
    {
        var statuses = new List<PaymentStatus>
        {
            PaymentStatus.Create("Paid", "Fully paid."),
            PaymentStatus.Create("Rejected", "Declined.")
        };
        SetupGetAll(statuses, 2);

        var result = await new GetAllPaymentStatusesQueryHandler(_repository, _cache)
            .Handle(new GetAllPaymentStatusesQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Select(r => r.Name).Should().BeEquivalentTo(["Paid", "Rejected"]);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnEmptyPagedResult()
    {
        SetupGetAll([], 0);

        var result = await new GetAllPaymentStatusesQueryHandler(_repository, _cache)
            .Handle(new GetAllPaymentStatusesQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_ShouldPassAllParametersToRepository()
    {
        SetupGetAll([], 0);

        await new GetAllPaymentStatusesQueryHandler(_repository, _cache)
            .Handle(new GetAllPaymentStatusesQuery(
                Search: "paid", IsActive: false, Page: 2, PageSize: 50,
                SortBy: "name", SortDirection: SortDirection.Desc),
            CancellationToken.None);

        await _repository.Received(1).GetAllAsync(
            "paid", false, "name", SortDirection.Desc, 2, 50, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(1, 10, 20, 2, false, true)]
    [InlineData(2, 10, 20, 2, true, false)]
    public async Task GetAll_ShouldComputeHasNextAndHasPreviousCorrectly(
        int page, int pageSize, int totalCount, int expectedTotalPages,
        bool expectedHasPrev, bool expectedHasNext)
    {
        SetupGetAll([], totalCount);

        var result = await new GetAllPaymentStatusesQueryHandler(_repository, _cache)
            .Handle(new GetAllPaymentStatusesQuery(Page: page, PageSize: pageSize), CancellationToken.None);

        result.TotalPages.Should().Be(expectedTotalPages);
        result.HasPreviousPage.Should().Be(expectedHasPrev);
        result.HasNextPage.Should().Be(expectedHasNext);
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturnResponse()
    {
        var id = Guid.NewGuid();
        var status = PaymentStatus.Create("Reserved", "Funds reserved.");
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(status);

        var result = await new GetPaymentStatusByIdQueryHandler(_repository)
            .Handle(new GetPaymentStatusByIdQuery(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Reserved");
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PaymentStatus?)null);

        var result = await new GetPaymentStatusByIdQueryHandler(_repository)
            .Handle(new GetPaymentStatusByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}

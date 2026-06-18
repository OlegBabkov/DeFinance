using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.MandatoryPayments.Queries;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.MandatoryPayments.Queries;

public class GetMandatoryPaymentQueryHandlerTests
{
    private readonly IMandatoryPaymentRepository _repository = Substitute.For<IMandatoryPaymentRepository>();

    private void SetupGetAll(IReadOnlyList<MandatoryPayment> items, int totalCount) =>
        _repository.GetAllAsync(
            Arg.Any<string?>(), Arg.Any<bool?>(),
            Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(),
            Arg.Any<PaymentFrequency?>(),
            Arg.Any<string?>(), Arg.Any<SortDirection>(),
            Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
        .Returns((items, totalCount));

    [Fact]
    public async Task GetAll_ShouldReturnPagedResultWithMappedItems()
    {
        var payments = new List<MandatoryPayment>
        {
            MandatoryPayment.Create("Rent", 1000m, Guid.NewGuid(), Guid.NewGuid(), null, null, PaymentFrequency.Monthly, 1, null, Guid.NewGuid()),
            MandatoryPayment.Create("Insurance", 200m, Guid.NewGuid(), Guid.NewGuid(), null, null, PaymentFrequency.Yearly, 1, null, Guid.NewGuid()),
        };
        SetupGetAll(payments, 2);

        var result = await new GetAllMandatoryPaymentsQueryHandler(_repository)
            .Handle(new GetAllMandatoryPaymentsQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Select(r => r.Name).Should().BeEquivalentTo(["Rent", "Insurance"]);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnEmptyPagedResult()
    {
        SetupGetAll([], 0);

        var result = await new GetAllMandatoryPaymentsQueryHandler(_repository)
            .Handle(new GetAllMandatoryPaymentsQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_ShouldPassFiltersToRepository()
    {
        SetupGetAll([], 0);
        var accountId = Guid.NewGuid();

        await new GetAllMandatoryPaymentsQueryHandler(_repository)
            .Handle(new GetAllMandatoryPaymentsQuery(
                Search: "rent", IsActive: true, AccountId: accountId,
                Frequency: PaymentFrequency.Monthly,
                Page: 2, PageSize: 10, SortBy: "name", SortDirection: SortDirection.Desc),
            CancellationToken.None);

        await _repository.Received(1).GetAllAsync(
            "rent", true,
            null, accountId, null, null,
            PaymentFrequency.Monthly,
            "name", SortDirection.Desc, 2, 10,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_WhenPaymentExists_ShouldReturnResponse()
    {
        var id = Guid.NewGuid();
        var payment = MandatoryPayment.Create("Rent", 1000m, Guid.NewGuid(), Guid.NewGuid(), null, null, PaymentFrequency.Monthly, 1, null, Guid.NewGuid());
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await new GetMandatoryPaymentByIdQueryHandler(_repository)
            .Handle(new GetMandatoryPaymentByIdQuery(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Rent");
        result.Amount.Should().Be(1000m);
    }

    [Fact]
    public async Task GetById_WhenPaymentNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MandatoryPayment?)null);

        var result = await new GetMandatoryPaymentByIdQueryHandler(_repository)
            .Handle(new GetMandatoryPaymentByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}

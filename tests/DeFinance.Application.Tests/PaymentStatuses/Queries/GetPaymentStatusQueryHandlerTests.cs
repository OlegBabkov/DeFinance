using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.PaymentStatuses.Queries;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.PaymentStatuses.Queries;

public class GetPaymentStatusQueryHandlerTests
{
    private readonly IPaymentStatusRepository _repository = Substitute.For<IPaymentStatusRepository>();

    [Fact]
    public async Task GetAll_ShouldReturnMappedResponses()
    {
        var statuses = new List<PaymentStatus>
        {
            PaymentStatus.Create("Paid", "Fully paid."),
            PaymentStatus.Create("Rejected", "Declined.")
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(statuses);

        var result = await new GetAllPaymentStatusesQueryHandler(_repository)
            .Handle(new GetAllPaymentStatusesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(r => r.Name).Should().BeEquivalentTo(["Paid", "Rejected"]);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([]);

        var result = await new GetAllPaymentStatusesQueryHandler(_repository)
            .Handle(new GetAllPaymentStatusesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
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

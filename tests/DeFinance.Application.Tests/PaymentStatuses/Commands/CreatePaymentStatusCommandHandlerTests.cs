using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.PaymentStatuses.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.PaymentStatuses.Commands;

public class CreatePaymentStatusCommandHandlerTests
{
    private readonly IPaymentStatusRepository _repository = Substitute.For<IPaymentStatusRepository>();
    private readonly CreatePaymentStatusCommandHandler _handler;

    public CreatePaymentStatusCommandHandlerTests()
    {
        _handler = new CreatePaymentStatusCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldCreatePaymentStatusAndReturnResponse()
    {
        var command = new CreatePaymentStatusCommand("Paid", "Transaction has been fully paid.");
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Paid");
        result.Description.Should().Be("Transaction has been fully paid.");
        result.IsActive.Should().BeTrue();

        await _repository.Received(1).AddAsync(Arg.Any<PaymentStatus>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullDescription_ShouldCreateSuccessfully()
    {
        var command = new CreatePaymentStatusCommand("Booked", null);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Booked");
        result.Description.Should().BeNull();
    }
}

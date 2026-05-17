using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.PaymentStatuses.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.PaymentStatuses.Commands;

public class UpdatePaymentStatusCommandHandlerTests
{
    private readonly IPaymentStatusRepository _repository = Substitute.For<IPaymentStatusRepository>();
    private readonly UpdatePaymentStatusCommandHandler _handler;

    public UpdatePaymentStatusCommandHandlerTests()
    {
        _handler = new UpdatePaymentStatusCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenExists_ShouldUpdateAndReturnResponse()
    {
        var id = Guid.NewGuid();
        var status = PaymentStatus.Create("Paid", "Old description");
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(status);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new UpdatePaymentStatusCommand(id, "Paid", "Updated description"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Paid");
        result.Description.Should().Be("Updated description");
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PaymentStatus?)null);

        var result = await _handler.Handle(new UpdatePaymentStatusCommand(Guid.NewGuid(), "Paid", null), CancellationToken.None);

        result.Should().BeNull();
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

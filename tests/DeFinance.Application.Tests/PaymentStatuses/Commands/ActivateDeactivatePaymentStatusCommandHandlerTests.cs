using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.PaymentStatuses.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.PaymentStatuses.Commands;

public class ActivateDeactivatePaymentStatusCommandHandlerTests
{
    private readonly IPaymentStatusRepository _repository = Substitute.For<IPaymentStatusRepository>();

    [Fact]
    public async Task Activate_WhenExists_ShouldSetIsActiveTrue()
    {
        var id = Guid.NewGuid();
        var status = PaymentStatus.Create("Rejected", null);
        status.Deactivate();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(status);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new ActivatePaymentStatusCommandHandler(_repository)
            .Handle(new ActivatePaymentStatusCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PaymentStatus?)null);

        var result = await new ActivatePaymentStatusCommandHandler(_repository)
            .Handle(new ActivatePaymentStatusCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Deactivate_WhenExists_ShouldSetIsActiveFalse()
    {
        var id = Guid.NewGuid();
        var status = PaymentStatus.Create("Paid", null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(status);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new DeactivatePaymentStatusCommandHandler(_repository)
            .Handle(new DeactivatePaymentStatusCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WhenNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PaymentStatus?)null);

        var result = await new DeactivatePaymentStatusCommandHandler(_repository)
            .Handle(new DeactivatePaymentStatusCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}

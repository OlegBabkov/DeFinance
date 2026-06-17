using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.MandatoryPayments.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.MandatoryPayments.Commands;

public class ActivateDeactivateMandatoryPaymentCommandHandlerTests
{
    private readonly IMandatoryPaymentRepository _repository = Substitute.For<IMandatoryPaymentRepository>();

    [Fact]
    public async Task Activate_WhenPaymentExists_ShouldSetIsActiveTrue()
    {
        var id = Guid.NewGuid();
        var payment = MandatoryPayment.Create("Rent", 500m, Guid.NewGuid(), Guid.NewGuid(), null, null, PaymentFrequency.Monthly, 1, null);
        payment.Deactivate();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(payment);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new ActivateMandatoryPaymentCommandHandler(_repository)
            .Handle(new ActivateMandatoryPaymentCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activate_WhenPaymentNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MandatoryPayment?)null);

        var result = await new ActivateMandatoryPaymentCommandHandler(_repository)
            .Handle(new ActivateMandatoryPaymentCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Deactivate_WhenPaymentExists_ShouldSetIsActiveFalse()
    {
        var id = Guid.NewGuid();
        var payment = MandatoryPayment.Create("Insurance", 200m, Guid.NewGuid(), Guid.NewGuid(), null, null, PaymentFrequency.Yearly, 1, null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(payment);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new DeactivateMandatoryPaymentCommandHandler(_repository)
            .Handle(new DeactivateMandatoryPaymentCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deactivate_WhenPaymentNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MandatoryPayment?)null);

        var result = await new DeactivateMandatoryPaymentCommandHandler(_repository)
            .Handle(new DeactivateMandatoryPaymentCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}

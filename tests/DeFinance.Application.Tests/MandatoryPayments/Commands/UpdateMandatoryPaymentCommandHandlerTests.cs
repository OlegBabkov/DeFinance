using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.MandatoryPayments.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.MandatoryPayments.Commands;

public class UpdateMandatoryPaymentCommandHandlerTests
{
    private readonly IMandatoryPaymentRepository _repository = Substitute.For<IMandatoryPaymentRepository>();
    private readonly UpdateMandatoryPaymentCommandHandler _handler;

    public UpdateMandatoryPaymentCommandHandlerTests()
    {
        _handler = new UpdateMandatoryPaymentCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenPaymentExists_ShouldUpdateAndReturnResponse()
    {
        var id = Guid.NewGuid();
        var payment = MandatoryPayment.Create("Old Name", 500m, Guid.NewGuid(), Guid.NewGuid(), null, null, PaymentFrequency.Monthly, 1, null, Guid.NewGuid());
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(payment);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var newCurrencyId = Guid.NewGuid();
        var newAccountId = Guid.NewGuid();
        var command = new UpdateMandatoryPaymentCommand(id, "New Name", 800m, newCurrencyId, newAccountId, null, null, PaymentFrequency.Quarterly, 5, "updated note");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.Amount.Should().Be(800m);
        result.CurrencyId.Should().Be(newCurrencyId);
        result.AccountId.Should().Be(newAccountId);
        result.Frequency.Should().Be(PaymentFrequency.Quarterly);
        result.DayOfPeriod.Should().Be(5);
        result.Notes.Should().Be("updated note");

        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MandatoryPayment?)null);
        var command = new UpdateMandatoryPaymentCommand(Guid.NewGuid(), "Name", 100m, Guid.NewGuid(), Guid.NewGuid(), null, null, PaymentFrequency.Monthly, 1, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeNull();
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

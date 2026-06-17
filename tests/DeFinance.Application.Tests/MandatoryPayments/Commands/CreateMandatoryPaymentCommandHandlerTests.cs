using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.MandatoryPayments.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.MandatoryPayments.Commands;

public class CreateMandatoryPaymentCommandHandlerTests
{
    private readonly IMandatoryPaymentRepository _repository = Substitute.For<IMandatoryPaymentRepository>();
    private readonly CreateMandatoryPaymentCommandHandler _handler;

    public CreateMandatoryPaymentCommandHandlerTests()
    {
        _handler = new CreateMandatoryPaymentCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldCreatePaymentAndReturnResponse()
    {
        var currencyId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var command = new CreateMandatoryPaymentCommand(
            "Rent", 1000m, currencyId, accountId, null, null,
            PaymentFrequency.Monthly, 1, "monthly rent");
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Rent");
        result.Amount.Should().Be(1000m);
        result.CurrencyId.Should().Be(currencyId);
        result.AccountId.Should().Be(accountId);
        result.Frequency.Should().Be(PaymentFrequency.Monthly);
        result.DayOfPeriod.Should().Be(1);
        result.Notes.Should().Be("monthly rent");
        result.IsActive.Should().BeTrue();

        await _repository.Received(1).AddAsync(Arg.Any<MandatoryPayment>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithOptionalIds_ShouldMapCategoryAndStatusIds()
    {
        var catId = Guid.NewGuid();
        var statusId = Guid.NewGuid();
        var command = new CreateMandatoryPaymentCommand(
            "Insurance", 200m, Guid.NewGuid(), Guid.NewGuid(), catId, statusId,
            PaymentFrequency.Quarterly, 15, null);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.CategoryId.Should().Be(catId);
        result.PaymentStatusId.Should().Be(statusId);
        result.Notes.Should().BeNull();
    }
}

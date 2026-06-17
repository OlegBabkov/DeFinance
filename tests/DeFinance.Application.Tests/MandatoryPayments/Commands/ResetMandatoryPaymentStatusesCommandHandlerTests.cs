using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.MandatoryPayments.Commands;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.MandatoryPayments.Commands;

public class ResetMandatoryPaymentStatusesCommandHandlerTests
{
    private readonly IMandatoryPaymentRepository _repository = Substitute.For<IMandatoryPaymentRepository>();
    private readonly ResetMandatoryPaymentStatusesCommandHandler _handler;

    public ResetMandatoryPaymentStatusesCommandHandlerTests()
    {
        _handler = new ResetMandatoryPaymentStatusesCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldDelegateToRepositoryAndReturnCount()
    {
        var accountId = Guid.NewGuid();
        _repository.ResetPaymentStatusesByAccountAsync(accountId, Arg.Any<CancellationToken>()).Returns(3);

        var result = await _handler.Handle(new ResetMandatoryPaymentStatusesCommand(accountId), CancellationToken.None);

        result.Should().Be(3);
        await _repository.Received(1).ResetPaymentStatusesByAccountAsync(accountId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoPaymentsReset_ShouldReturnZero()
    {
        var accountId = Guid.NewGuid();
        _repository.ResetPaymentStatusesByAccountAsync(accountId, Arg.Any<CancellationToken>()).Returns(0);

        var result = await _handler.Handle(new ResetMandatoryPaymentStatusesCommand(accountId), CancellationToken.None);

        result.Should().Be(0);
    }
}

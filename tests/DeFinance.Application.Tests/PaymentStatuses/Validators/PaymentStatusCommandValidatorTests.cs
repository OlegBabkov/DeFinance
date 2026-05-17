using DeFinance.Application.PaymentStatuses.Commands;
using FluentAssertions;

namespace DeFinance.Application.Tests.PaymentStatuses.Validators;

public class PaymentStatusCommandValidatorTests
{
    private readonly CreatePaymentStatusCommandValidator _createValidator = new();
    private readonly UpdatePaymentStatusCommandValidator _updateValidator = new();

    [Fact]
    public async Task CreateValidator_WithValidCommand_ShouldBeValid()
    {
        var result = await _createValidator.ValidateAsync(new CreatePaymentStatusCommand("Paid", "Description"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateValidator_WithNullDescription_ShouldBeValid()
    {
        var result = await _createValidator.ValidateAsync(new CreatePaymentStatusCommand("Paid", null));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateValidator_WithEmptyName_ShouldBeInvalid(string name)
    {
        var result = await _createValidator.ValidateAsync(new CreatePaymentStatusCommand(name, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePaymentStatusCommand.Name));
    }

    [Fact]
    public async Task CreateValidator_WithNameExceeding100Chars_ShouldBeInvalid()
    {
        var result = await _createValidator.ValidateAsync(new CreatePaymentStatusCommand(new string('A', 101), null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePaymentStatusCommand.Name));
    }

    [Fact]
    public async Task CreateValidator_WithDescriptionExceeding500Chars_ShouldBeInvalid()
    {
        var result = await _createValidator.ValidateAsync(new CreatePaymentStatusCommand("Paid", new string('X', 501)));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePaymentStatusCommand.Description));
    }

    [Fact]
    public async Task UpdateValidator_WithValidCommand_ShouldBeValid()
    {
        var result = await _updateValidator.ValidateAsync(new UpdatePaymentStatusCommand(Guid.NewGuid(), "Booked", null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateValidator_WithEmptyName_ShouldBeInvalid()
    {
        var result = await _updateValidator.ValidateAsync(new UpdatePaymentStatusCommand(Guid.NewGuid(), "", null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePaymentStatusCommand.Name));
    }
}

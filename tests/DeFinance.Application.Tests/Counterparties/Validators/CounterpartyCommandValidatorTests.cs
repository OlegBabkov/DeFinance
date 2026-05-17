using DeFinance.Application.Counterparties.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;

namespace DeFinance.Application.Tests.Counterparties.Validators;

public class CounterpartyCommandValidatorTests
{
    private readonly CreateCounterpartyCommandValidator _createValidator = new();
    private readonly UpdateCounterpartyCommandValidator _updateValidator = new();

    [Fact]
    public async Task CreateValidator_WithValidCommand_ShouldBeValid()
    {
        var result = await _createValidator.ValidateAsync(new CreateCounterpartyCommand("Lidl", CounterpartyType.Company, null));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateValidator_WithEmptyName_ShouldBeInvalid(string name)
    {
        var result = await _createValidator.ValidateAsync(new CreateCounterpartyCommand(name, CounterpartyType.Company, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCounterpartyCommand.Name));
    }

    [Fact]
    public async Task CreateValidator_WithNameExceeding100Chars_ShouldBeInvalid()
    {
        var result = await _createValidator.ValidateAsync(new CreateCounterpartyCommand(new string('A', 101), CounterpartyType.Company, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCounterpartyCommand.Name));
    }

    [Fact]
    public async Task CreateValidator_WithContactInfoExceeding500Chars_ShouldBeInvalid()
    {
        var result = await _createValidator.ValidateAsync(new CreateCounterpartyCommand("Lidl", CounterpartyType.Company, new string('X', 501)));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCounterpartyCommand.ContactInfo));
    }

    [Fact]
    public async Task CreateValidator_WithInvalidType_ShouldBeInvalid()
    {
        var result = await _createValidator.ValidateAsync(new CreateCounterpartyCommand("Test", (CounterpartyType)99, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCounterpartyCommand.Type));
    }

    [Fact]
    public async Task UpdateValidator_WithValidCommand_ShouldBeValid()
    {
        var result = await _updateValidator.ValidateAsync(new UpdateCounterpartyCommand(Guid.NewGuid(), "Rewe", CounterpartyType.Company, null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateValidator_WithEmptyName_ShouldBeInvalid()
    {
        var result = await _updateValidator.ValidateAsync(new UpdateCounterpartyCommand(Guid.NewGuid(), "", CounterpartyType.Company, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateCounterpartyCommand.Name));
    }
}

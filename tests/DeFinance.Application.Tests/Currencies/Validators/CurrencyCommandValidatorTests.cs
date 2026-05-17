using DeFinance.Application.Currencies.Commands;
using FluentAssertions;

namespace DeFinance.Application.Tests.Currencies.Validators;

public class CurrencyCommandValidatorTests
{
    private readonly CreateCurrencyCommandValidator _createValidator = new();
    private readonly UpdateCurrencyCommandValidator _updateValidator = new();

    [Fact]
    public async Task CreateValidator_WithValidCommand_ShouldBeValid()
    {
        var result = await _createValidator.ValidateAsync(new CreateCurrencyCommand("USD", "US Dollar", "$"));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "US Dollar", "$")]
    [InlineData("TOOLONGCODE", "US Dollar", "$")]
    [InlineData("123", "US Dollar", "$")]
    [InlineData("U$D", "US Dollar", "$")]
    public async Task CreateValidator_WithInvalidCode_ShouldBeInvalid(string code, string name, string symbol)
    {
        var result = await _createValidator.ValidateAsync(new CreateCurrencyCommand(code, name, symbol));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCurrencyCommand.Code));
    }

    [Theory]
    [InlineData("USD", "", "$")]
    [InlineData("USD", "US Dollar", "")]
    public async Task CreateValidator_WithEmptyNameOrSymbol_ShouldBeInvalid(string code, string name, string symbol)
    {
        var result = await _createValidator.ValidateAsync(new CreateCurrencyCommand(code, name, symbol));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task CreateValidator_WithNameExceeding100Chars_ShouldBeInvalid()
    {
        var longName = new string('A', 101);
        var result = await _createValidator.ValidateAsync(new CreateCurrencyCommand("USD", longName, "$"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCurrencyCommand.Name));
    }

    [Fact]
    public async Task UpdateValidator_WithValidCommand_ShouldBeValid()
    {
        var result = await _updateValidator.ValidateAsync(new UpdateCurrencyCommand(Guid.NewGuid(), "US Dollar", "$"));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "$")]
    [InlineData("US Dollar", "")]
    public async Task UpdateValidator_WithEmptyNameOrSymbol_ShouldBeInvalid(string name, string symbol)
    {
        var result = await _updateValidator.ValidateAsync(new UpdateCurrencyCommand(Guid.NewGuid(), name, symbol));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateValidator_WithSymbolExceeding10Chars_ShouldBeInvalid()
    {
        var longSymbol = new string('$', 11);
        var result = await _updateValidator.ValidateAsync(new UpdateCurrencyCommand(Guid.NewGuid(), "Dollar", longSymbol));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateCurrencyCommand.Symbol));
    }
}

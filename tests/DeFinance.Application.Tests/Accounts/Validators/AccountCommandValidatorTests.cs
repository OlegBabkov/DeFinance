using DeFinance.Application.Accounts.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;

namespace DeFinance.Application.Tests.Accounts.Validators;

public class AccountCommandValidatorTests
{
    private readonly CreateAccountCommandValidator _createValidator = new();
    private readonly UpdateAccountCommandValidator _updateValidator = new();

    [Fact]
    public async Task CreateValidator_WithValidCommand_ShouldBeValid()
    {
        var result = await _createValidator.ValidateAsync(
            new CreateAccountCommand("My Account", AccountType.Savings, 100m, Guid.NewGuid()));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateValidator_WithEmptyName_ShouldBeInvalid(string name)
    {
        var result = await _createValidator.ValidateAsync(
            new CreateAccountCommand(name, AccountType.Checking, 0m, Guid.NewGuid()));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAccountCommand.Name));
    }

    [Fact]
    public async Task CreateValidator_WithNameExceeding100Chars_ShouldBeInvalid()
    {
        var result = await _createValidator.ValidateAsync(
            new CreateAccountCommand(new string('A', 101), AccountType.Checking, 0m, Guid.NewGuid()));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAccountCommand.Name));
    }

    [Fact]
    public async Task CreateValidator_WithNegativeInitialBalance_ShouldBeInvalid()
    {
        var result = await _createValidator.ValidateAsync(
            new CreateAccountCommand("Account", AccountType.Savings, -1m, Guid.NewGuid()));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAccountCommand.InitialBalance));
    }

    [Fact]
    public async Task CreateValidator_WithEmptyCurrencyId_ShouldBeInvalid()
    {
        var result = await _createValidator.ValidateAsync(
            new CreateAccountCommand("Account", AccountType.Savings, 0m, Guid.Empty));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAccountCommand.CurrencyId));
    }

    [Fact]
    public async Task UpdateValidator_WithValidCommand_ShouldBeValid()
    {
        var result = await _updateValidator.ValidateAsync(
            new UpdateAccountCommand(Guid.NewGuid(), "Updated Name"));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateValidator_WithEmptyName_ShouldBeInvalid(string name)
    {
        var result = await _updateValidator.ValidateAsync(
            new UpdateAccountCommand(Guid.NewGuid(), name));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAccountCommand.Name));
    }
}

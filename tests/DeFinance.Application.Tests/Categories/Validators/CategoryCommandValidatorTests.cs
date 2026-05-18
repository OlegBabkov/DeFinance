using DeFinance.Application.Categories.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;

namespace DeFinance.Application.Tests.Categories.Validators;

public class CategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _createValidator = new();
    private readonly UpdateCategoryCommandValidator _updateValidator = new();

    [Fact]
    public async Task CreateValidator_WithValidCommand_ShouldBeValid()
    {
        var result = await _createValidator.ValidateAsync(
            new CreateCategoryCommand("Food", CategoryType.Expense, "#FF5733", "🍔", null, null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateValidator_WithNoOptionals_ShouldBeValid()
    {
        var result = await _createValidator.ValidateAsync(
            new CreateCategoryCommand("Salary", CategoryType.Income, null, null, null, null));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateValidator_WithEmptyName_ShouldBeInvalid(string name)
    {
        var result = await _createValidator.ValidateAsync(
            new CreateCategoryCommand(name, CategoryType.Expense, null, null, null, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCategoryCommand.Name));
    }

    [Theory]
    [InlineData("FF5733")]
    [InlineData("red")]
    [InlineData("#ZZZZZZ")]
    [InlineData("#12345")]
    public async Task CreateValidator_WithInvalidColor_ShouldBeInvalid(string color)
    {
        var result = await _createValidator.ValidateAsync(
            new CreateCategoryCommand("Food", CategoryType.Expense, color, null, null, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCategoryCommand.Color));
    }

    [Theory]
    [InlineData("#FF5733")]
    [InlineData("#FFF")]
    [InlineData("#abc")]
    [InlineData("#AABBCC")]
    public async Task CreateValidator_WithValidColor_ShouldBeValid(string color)
    {
        var result = await _createValidator.ValidateAsync(
            new CreateCategoryCommand("Food", CategoryType.Expense, color, null, null, null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateValidator_WithIconExceeding50Chars_ShouldBeInvalid()
    {
        var result = await _createValidator.ValidateAsync(
            new CreateCategoryCommand("Food", CategoryType.Expense, null, new string('x', 51), null, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCategoryCommand.Icon));
    }

    [Fact]
    public async Task UpdateValidator_WithValidCommand_ShouldBeValid()
    {
        var result = await _updateValidator.ValidateAsync(
            new UpdateCategoryCommand(Guid.NewGuid(), "Updated", "#00FF00", null, null));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateValidator_WithEmptyName_ShouldBeInvalid(string name)
    {
        var result = await _updateValidator.ValidateAsync(
            new UpdateCategoryCommand(Guid.NewGuid(), name, null, null, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateCategoryCommand.Name));
    }
}

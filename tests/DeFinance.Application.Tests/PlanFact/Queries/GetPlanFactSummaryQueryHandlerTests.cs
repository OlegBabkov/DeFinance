using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.PlanFact.Queries;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.PlanFact.Queries;

public class GetPlanFactSummaryQueryHandlerTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IBudgetEntryRepository _budgetEntryRepository = Substitute.For<IBudgetEntryRepository>();
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IOpeningBalanceOverrideRepository _openingBalanceRepository = Substitute.For<IOpeningBalanceOverrideRepository>();
    private readonly GetPlanFactSummaryQueryHandler _handler;

    public GetPlanFactSummaryQueryHandlerTests()
    {
        _handler = new GetPlanFactSummaryQueryHandler(
            _categoryRepository, _budgetEntryRepository,
            _transactionRepository, _openingBalanceRepository);
    }

    private void SetupDefaults(int year, IReadOnlyList<int> months)
    {
        _categoryRepository.GetActiveByTypesAsync(Arg.Any<IReadOnlyList<CategoryType>>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _budgetEntryRepository.GetByPeriodAsync(year, months, Arg.Any<CancellationToken>())
            .Returns([]);
        _transactionRepository.GetCategoryMonthlyTotalsAsync(year, months, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _openingBalanceRepository.GetByYearAsync(year, months, Arg.Any<CancellationToken>())
            .Returns([]);
        _transactionRepository.GetSignedBalanceBeforeAsync(Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(0m);
    }

    [Fact]
    public async Task Handle_ShouldReturnOneMonthDataPerRequestedMonth()
    {
        SetupDefaults(2025, [3, 6]);

        var result = await _handler.Handle(new GetPlanFactSummaryQuery(2025, [3, 6]), CancellationToken.None);

        result.Months.Should().HaveCount(2);
        result.Months[0].Month.Should().Be(3);
        result.Months[1].Month.Should().Be(6);
    }

    [Fact]
    public async Task Handle_ShouldDeduplicateAndSortMonths()
    {
        SetupDefaults(2025, [6, 3, 6]);
        // handler uses .Distinct().OrderBy(m => m)
        _budgetEntryRepository.GetByPeriodAsync(2025, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _transactionRepository.GetCategoryMonthlyTotalsAsync(2025, Arg.Any<IReadOnlyList<int>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _openingBalanceRepository.GetByYearAsync(2025, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _handler.Handle(new GetPlanFactSummaryQuery(2025, [6, 3, 6]), CancellationToken.None);

        result.Months.Should().HaveCount(2);
        result.Months[0].Month.Should().Be(3);
        result.Months[1].Month.Should().Be(6);
    }

    [Fact]
    public async Task Handle_WithOpeningBalanceOverride_ShouldUseOverrideAmount()
    {
        _categoryRepository.GetActiveByTypesAsync(Arg.Any<IReadOnlyList<CategoryType>>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _budgetEntryRepository.GetByPeriodAsync(2025, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>()).Returns([]);
        _transactionRepository.GetCategoryMonthlyTotalsAsync(2025, Arg.Any<IReadOnlyList<int>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);
        var overrides = (IReadOnlyList<OpeningBalanceOverride>)[OpeningBalanceOverride.Create(2025, 5, 7500m, Guid.NewGuid())];
        _openingBalanceRepository.GetByYearAsync(2025, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>()).Returns(overrides);

        var result = await _handler.Handle(new GetPlanFactSummaryQuery(2025, [5]), CancellationToken.None);

        result.Months[0].OpeningBalance.Should().Be(7500m);
        result.Months[0].OpeningBalanceIsOverride.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithoutOpeningBalanceOverride_ShouldUseTransactionBalance()
    {
        _categoryRepository.GetActiveByTypesAsync(Arg.Any<IReadOnlyList<CategoryType>>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _budgetEntryRepository.GetByPeriodAsync(2025, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>()).Returns([]);
        _transactionRepository.GetCategoryMonthlyTotalsAsync(2025, Arg.Any<IReadOnlyList<int>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _openingBalanceRepository.GetByYearAsync(2025, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>()).Returns([]);
        _transactionRepository.GetSignedBalanceBeforeAsync(Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(3200m);

        var result = await _handler.Handle(new GetPlanFactSummaryQuery(2025, [4]), CancellationToken.None);

        result.Months[0].OpeningBalance.Should().Be(3200m);
        result.Months[0].OpeningBalanceIsOverride.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldMapIncomeAndExpenseCategoriesIntoRows()
    {
        var income = Category.Create("Salary", CategoryType.Income, null, null, null, null, Guid.NewGuid());
        var expense = Category.Create("Rent", CategoryType.Expense, null, null, null, null, Guid.NewGuid());

        _categoryRepository.GetActiveByTypesAsync(Arg.Any<IReadOnlyList<CategoryType>>(), Arg.Any<CancellationToken>())
            .Returns([income, expense]);
        _budgetEntryRepository.GetByPeriodAsync(2025, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>()).Returns([]);
        _transactionRepository.GetCategoryMonthlyTotalsAsync(2025, Arg.Any<IReadOnlyList<int>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _openingBalanceRepository.GetByYearAsync(2025, Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>()).Returns([]);
        _transactionRepository.GetSignedBalanceBeforeAsync(Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(0m);

        var result = await _handler.Handle(new GetPlanFactSummaryQuery(2025, [1]), CancellationToken.None);

        var month = result.Months[0];
        month.IncomeCategories.Should().HaveCount(1);
        month.IncomeCategories[0].CategoryName.Should().Be("Salary");
        month.ExpenseCategories.Should().HaveCount(1);
        month.ExpenseCategories[0].CategoryName.Should().Be("Rent");
    }
}

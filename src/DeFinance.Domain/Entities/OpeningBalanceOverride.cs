namespace DeFinance.Domain.Entities;

public class OpeningBalanceOverride
{
    public Guid Id { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal? Amount { get; private set; }
    public decimal? PlanAmount { get; private set; }
    public Guid UserId { get; private set; }

    private OpeningBalanceOverride() { }

    public static OpeningBalanceOverride Create(int year, int month, decimal amount, Guid userId) =>
        new() { Id = Guid.NewGuid(), Year = year, Month = month, Amount = amount, UserId = userId };

    public static OpeningBalanceOverride CreateForPlan(int year, int month, decimal planAmount, Guid userId) =>
        new() { Id = Guid.NewGuid(), Year = year, Month = month, PlanAmount = planAmount, UserId = userId };

    public void UpdateAmount(decimal amount) => Amount = amount;
    public void UpdatePlanAmount(decimal planAmount) => PlanAmount = planAmount;
}

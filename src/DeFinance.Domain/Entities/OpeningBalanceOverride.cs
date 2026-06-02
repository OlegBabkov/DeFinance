namespace DeFinance.Domain.Entities;

public class OpeningBalanceOverride
{
    public Guid Id { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal? Amount { get; private set; }
    public decimal? PlanAmount { get; private set; }

    private OpeningBalanceOverride() { }

    public static OpeningBalanceOverride Create(int year, int month, decimal amount) =>
        new() { Id = Guid.NewGuid(), Year = year, Month = month, Amount = amount };

    public static OpeningBalanceOverride CreateForPlan(int year, int month, decimal planAmount) =>
        new() { Id = Guid.NewGuid(), Year = year, Month = month, PlanAmount = planAmount };

    public void UpdateAmount(decimal amount) => Amount = amount;
    public void UpdatePlanAmount(decimal planAmount) => PlanAmount = planAmount;
}

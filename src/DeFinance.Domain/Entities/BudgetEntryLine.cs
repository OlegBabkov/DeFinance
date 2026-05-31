namespace DeFinance.Domain.Entities;

public class BudgetEntryLine
{
    public Guid Id { get; private set; }
    public Guid BudgetEntryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public int Order { get; private set; }

    private BudgetEntryLine() { }

    public static BudgetEntryLine Create(Guid budgetEntryId, string name, decimal amount, int order) =>
        new()
        {
            Id = Guid.NewGuid(),
            BudgetEntryId = budgetEntryId,
            Name = name,
            Amount = amount,
            Order = order,
        };
}

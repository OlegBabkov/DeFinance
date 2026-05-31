namespace DeFinance.Domain.Entities;

public class BudgetEntry
{
    public Guid Id { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal PlannedAmount { get; private set; }

    private readonly List<BudgetEntryLine> _lines = [];
    public IReadOnlyList<BudgetEntryLine> Lines => _lines.AsReadOnly();

    private BudgetEntry() { }

    public static BudgetEntry Create(Guid categoryId, int year, int month, decimal plannedAmount) =>
        new()
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Year = year,
            Month = month,
            PlannedAmount = plannedAmount,
        };

    public void UpdateAmount(decimal plannedAmount) => PlannedAmount = plannedAmount;

    public void UpdateLines(IEnumerable<(string Name, decimal Amount)> lines)
    {
        _lines.Clear();
        var order = 0;
        foreach (var (name, amount) in lines)
            _lines.Add(BudgetEntryLine.Create(Id, name, amount, order++));
    }
}

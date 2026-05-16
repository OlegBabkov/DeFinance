namespace DeFinance.Domain.Entities;

public class Currency
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Symbol { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private Currency() { }

    public static Currency Create(string code, string name, string symbol) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = code.ToUpperInvariant(),
            Name = name,
            Symbol = symbol,
            IsActive = true
        };

    public void Update(string name, string symbol)
    {
        Name = name;
        Symbol = symbol;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

namespace DeFinance.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public AccountType Type { get; private set; }
    public decimal Balance { get; private set; }
    public Guid CurrencyId { get; private set; }
    public Currency? Currency { get; private set; }
    public bool IsActive { get; private set; }

    private Account() { }

    public static Account Create(string name, AccountType type, decimal initialBalance, Guid currencyId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            Balance = initialBalance,
            CurrencyId = currencyId,
            IsActive = true
        };

    public void Update(string name) => Name = name;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

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
    public Guid UserId { get; private set; }
    public int SortOrder { get; private set; }

    private Account() { }

    public static Account Create(string name, AccountType type, decimal initialBalance, Guid currencyId, Guid userId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            Balance = initialBalance,
            CurrencyId = currencyId,
            UserId = userId,
            IsActive = true
        };

    public void SetSortOrder(int order) => SortOrder = order;

    public void Update(string name) => Name = name;
    public void AdjustBalance(decimal delta) => Balance += delta;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

namespace DeFinance.Domain.Entities;

public class Counterparty
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public CounterpartyType Type { get; private set; }
    public string? ContactInfo { get; private set; }
    public bool IsActive { get; private set; }

    private Counterparty() { }

    public static Counterparty Create(string name, CounterpartyType type, string? contactInfo) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            ContactInfo = contactInfo,
            IsActive = true
        };

    public void Update(string name, CounterpartyType type, string? contactInfo)
    {
        Name = name;
        Type = type;
        ContactInfo = contactInfo;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

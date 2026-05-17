namespace DeFinance.Domain.Entities;

public class PaymentStatus
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private PaymentStatus() { }

    public static PaymentStatus Create(string name, string? description) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsActive = true
        };

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

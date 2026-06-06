namespace DeFinance.Domain.Entities;

public class PaymentStatus
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Color { get; private set; }
    public bool IsActive { get; private set; }

    private PaymentStatus() { }

    public static PaymentStatus Create(string name, string? description, string? color = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Color = color,
            IsActive = true
        };

    public void Update(string name, string? description, string? color)
    {
        Name = name;
        Description = description;
        Color = color;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

namespace DeFinance.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public CategoryType Type { get; private set; }
    public string? Color { get; private set; }
    public string? Icon { get; private set; }
    public Guid? ParentId { get; private set; }
    public bool IsActive { get; private set; }

    private Category() { }

    public static Category Create(string name, CategoryType type, string? color, string? icon, Guid? parentId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            Color = color,
            Icon = icon,
            ParentId = parentId,
            IsActive = true
        };

    public void Update(string name, string? color, string? icon)
    {
        Name = name;
        Color = color;
        Icon = icon;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

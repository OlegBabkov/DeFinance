namespace DeFinance.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }
    public byte[]? Photo { get; private set; }
    public string? PhotoContentType { get; private set; }

    private User() { }

    public static User Create(string username, string password, string email, string? phoneNumber) =>
        new()
        {
            Id = Guid.NewGuid(),
            Username = username,
            Password = password,
            Email = email,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

    public void Update(string username, string email, string? phoneNumber)
    {
        Username = username;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public void ChangePassword(string password) => Password = password;

    public void SetPhoto(byte[] photo, string contentType)
    {
        Photo = photo;
        PhotoContentType = contentType;
    }

    public void RemovePhoto()
    {
        Photo = null;
        PhotoContentType = null;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

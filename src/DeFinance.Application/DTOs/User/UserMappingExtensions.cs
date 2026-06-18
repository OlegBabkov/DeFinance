namespace DeFinance.Application.DTOs.User;

public static class UserMappingExtensions
{
    public static UserResponse ToResponse(this Domain.Entities.User user) =>
        new(user.Id, user.Username, user.Email, user.PhoneNumber, user.CreatedAt, user.IsActive,
            user.Photo is not null ? $"data:{user.PhotoContentType};base64,{Convert.ToBase64String(user.Photo)}" : null);

    public static IReadOnlyList<UserResponse> ToResponse(this IEnumerable<Domain.Entities.User> users) =>
        users.Select(u => u.ToResponse()).ToList();
}

namespace DeFinance.Application.DTOs.User;

public static class UserMappingExtensions
{
    public static UserResponse ToResponse(this Domain.Entities.User user) =>
        new(user.Id, user.Username, user.CreatedAt, user.IsActive);

    public static IReadOnlyList<UserResponse> ToResponse(this IEnumerable<Domain.Entities.User> users) =>
        users.Select(u => u.ToResponse()).ToList();
}

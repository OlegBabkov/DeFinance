namespace DeFinance.Application.DTOs.PaymentStatus;

public static class PaymentStatusMappingExtensions
{
    public static PaymentStatusResponse ToResponse(this Domain.Entities.PaymentStatus ps) =>
        new(ps.Id, ps.Name, ps.Description, ps.IsActive);

    public static IReadOnlyList<PaymentStatusResponse> ToResponse(this IEnumerable<Domain.Entities.PaymentStatus> statuses) =>
        statuses.Select(s => s.ToResponse()).ToList();
}

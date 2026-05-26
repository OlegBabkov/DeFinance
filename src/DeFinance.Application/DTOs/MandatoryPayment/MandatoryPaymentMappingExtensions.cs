using DeFinance.Application.DTOs.Account;
using DeFinance.Application.DTOs.Category;
using DeFinance.Application.DTOs.Currency;
using DeFinance.Application.DTOs.PaymentStatus;

namespace DeFinance.Application.DTOs.MandatoryPayment;

public static class MandatoryPaymentMappingExtensions
{
    public static MandatoryPaymentResponse ToResponse(this Domain.Entities.MandatoryPayment p) =>
        new(p.Id, p.Name, p.Amount,
            p.CurrencyId, p.Currency?.ToResponse(),
            p.AccountId,  p.Account?.ToResponse(),
            p.CategoryId, p.Category?.ToResponse(),
            p.PaymentStatusId, p.PaymentStatus?.ToResponse(),
            p.Frequency, p.DayOfPeriod, p.Notes, p.IsActive);

    public static IReadOnlyList<MandatoryPaymentResponse> ToResponse(
        this IEnumerable<Domain.Entities.MandatoryPayment> payments) =>
        payments.Select(p => p.ToResponse()).ToList();
}

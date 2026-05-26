using DeFinance.Application.DTOs.Account;
using DeFinance.Application.DTOs.Category;
using DeFinance.Application.DTOs.Currency;
using DeFinance.Application.DTOs.PaymentStatus;
using DeFinance.Domain.Entities;

namespace DeFinance.Application.DTOs.MandatoryPayment;

public record MandatoryPaymentResponse(
    Guid Id,
    string Name,
    decimal Amount,
    Guid CurrencyId,
    CurrencyResponse? Currency,
    Guid AccountId,
    AccountResponse? Account,
    Guid? CategoryId,
    CategoryResponse? Category,
    Guid? PaymentStatusId,
    PaymentStatusResponse? PaymentStatus,
    PaymentFrequency Frequency,
    int DayOfPeriod,
    string? Notes,
    bool IsActive);

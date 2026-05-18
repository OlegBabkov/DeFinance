using DeFinance.Application.DTOs.Account;
using DeFinance.Application.DTOs.Category;
using DeFinance.Application.DTOs.Counterparty;
using DeFinance.Application.DTOs.Currency;
using DeFinance.Application.DTOs.PaymentStatus;

namespace DeFinance.Application.DTOs.Transaction;

public static class TransactionMappingExtensions
{
    public static TransactionResponse ToResponse(this Domain.Entities.Transaction t) =>
        new(t.Id,
            t.DateTime,
            t.Sum,
            t.ExchangeRate,
            t.AmountInCurrency,
            t.InCurrencyId,
            t.InCurrency?.ToResponse(),
            t.AccountId,
            t.Account?.ToResponse(),
            t.CategoryId,
            t.Category?.ToResponse(),
            t.CounterpartyId,
            t.Counterparty?.ToResponse(),
            t.PaymentStatusId,
            t.PaymentStatus?.ToResponse(),
            t.Notes);

    public static IReadOnlyList<TransactionResponse> ToResponse(this IEnumerable<Domain.Entities.Transaction> transactions) =>
        transactions.Select(t => t.ToResponse()).ToList();
}

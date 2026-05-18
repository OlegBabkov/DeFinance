using DeFinance.Application.DTOs.Account;
using DeFinance.Application.DTOs.Category;
using DeFinance.Application.DTOs.Counterparty;
using DeFinance.Application.DTOs.Currency;
using DeFinance.Application.DTOs.PaymentStatus;

namespace DeFinance.Application.DTOs.Transaction;

public record TransactionResponse(
    Guid Id,
    DateTime DateTime,
    decimal Sum,
    decimal ExchangeRate,
    decimal AmountInCurrency,
    Guid InCurrencyId,
    CurrencyResponse? InCurrency,
    Guid AccountId,
    AccountResponse? Account,
    Guid CategoryId,
    CategoryResponse? Category,
    Guid? CounterpartyId,
    CounterpartyResponse? Counterparty,
    Guid PaymentStatusId,
    PaymentStatusResponse? PaymentStatus,
    string? Notes);

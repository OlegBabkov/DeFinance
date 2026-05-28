namespace DeFinance.Application.DTOs.Transaction;

public record TransactionListResponse(
    IReadOnlyList<TransactionResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage,
    decimal TotalSum,
    decimal TotalAmountInCurrency
);

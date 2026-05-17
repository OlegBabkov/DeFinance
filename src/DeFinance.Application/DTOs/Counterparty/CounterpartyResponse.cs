using DeFinance.Domain.Entities;

namespace DeFinance.Application.DTOs.Counterparty;

public record CounterpartyResponse(
    Guid Id,
    string Name,
    CounterpartyType Type,
    string? ContactInfo,
    bool IsActive);

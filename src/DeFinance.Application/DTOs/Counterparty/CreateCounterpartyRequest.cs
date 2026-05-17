using DeFinance.Domain.Entities;

namespace DeFinance.Application.DTOs.Counterparty;

public record CreateCounterpartyRequest(
    string Name,
    CounterpartyType Type,
    string? ContactInfo);

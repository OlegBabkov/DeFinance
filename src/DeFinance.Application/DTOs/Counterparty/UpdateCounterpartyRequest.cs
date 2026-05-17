using DeFinance.Domain.Entities;

namespace DeFinance.Application.DTOs.Counterparty;

public record UpdateCounterpartyRequest(
    string Name,
    CounterpartyType Type,
    string? ContactInfo);

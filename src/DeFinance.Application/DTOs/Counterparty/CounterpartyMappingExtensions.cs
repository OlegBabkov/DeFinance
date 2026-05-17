namespace DeFinance.Application.DTOs.Counterparty;

public static class CounterpartyMappingExtensions
{
    public static CounterpartyResponse ToResponse(this Domain.Entities.Counterparty counterparty) =>
        new(counterparty.Id, counterparty.Name, counterparty.Type, counterparty.ContactInfo, counterparty.IsActive);

    public static IReadOnlyList<CounterpartyResponse> ToResponse(this IEnumerable<Domain.Entities.Counterparty> counterparties) =>
        counterparties.Select(c => c.ToResponse()).ToList();
}

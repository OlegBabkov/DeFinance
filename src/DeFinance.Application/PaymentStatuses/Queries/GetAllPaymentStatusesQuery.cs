using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.PaymentStatus;
using MediatR;

namespace DeFinance.Application.PaymentStatuses.Queries;

public record GetAllPaymentStatusesQuery : IRequest<IReadOnlyList<PaymentStatusResponse>>;

public class GetAllPaymentStatusesQueryHandler(IPaymentStatusRepository repository)
    : IRequestHandler<GetAllPaymentStatusesQuery, IReadOnlyList<PaymentStatusResponse>>
{
    public async Task<IReadOnlyList<PaymentStatusResponse>> Handle(GetAllPaymentStatusesQuery request, CancellationToken cancellationToken) =>
        (await repository.GetAllAsync(cancellationToken)).ToResponse();
}

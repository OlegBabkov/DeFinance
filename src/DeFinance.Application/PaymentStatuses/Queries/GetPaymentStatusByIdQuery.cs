using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.DTOs.PaymentStatus;
using MediatR;

namespace DeFinance.Application.PaymentStatuses.Queries;

public record GetPaymentStatusByIdQuery(Guid Id) : IRequest<PaymentStatusResponse?>;

public class GetPaymentStatusByIdQueryHandler(IPaymentStatusRepository repository)
    : IRequestHandler<GetPaymentStatusByIdQuery, PaymentStatusResponse?>
{
    public async Task<PaymentStatusResponse?> Handle(GetPaymentStatusByIdQuery request, CancellationToken cancellationToken)
    {
        var status = await repository.GetByIdAsync(request.Id, cancellationToken);
        return status?.ToResponse();
    }
}

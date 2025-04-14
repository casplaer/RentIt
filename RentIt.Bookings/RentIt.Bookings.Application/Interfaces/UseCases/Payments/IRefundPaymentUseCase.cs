using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Application.Interfaces.UseCases.Payments
{
    public interface IRefundPaymentUseCase
    {
        Task<Payment> ExecuteAsync(Guid paymentId, bool isFined, CancellationToken cancellationToken);
    }
}

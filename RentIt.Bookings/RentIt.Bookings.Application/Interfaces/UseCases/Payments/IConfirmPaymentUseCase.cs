using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Application.Interfaces.UseCases.Payments
{
    public interface IConfirmPaymentUseCase
    {
        Task<Payment> ExecuteAsync(Guid paymentId, CancellationToken cancellationToken);
    }
}

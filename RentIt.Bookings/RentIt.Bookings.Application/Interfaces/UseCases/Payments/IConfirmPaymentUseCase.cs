using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Application.Interfaces.UseCases.Payments
{
    public interface IConfirmPaymentUseCase
    {
        Task<Payment> ExecuteAsync(Guid bookingId, string userId, CancellationToken cancellationToken);
    }
}

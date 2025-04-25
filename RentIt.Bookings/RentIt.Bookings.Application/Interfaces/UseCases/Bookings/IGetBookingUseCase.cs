using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Application.Interfaces.UseCases.Bookings
{
    public interface IGetBookingUseCase
    {
        Task<Booking> ExecuteAsync(
            Guid bookingId, 
            string authenticatedUserId,
            string authenticatedUserRole, 
            CancellationToken cancellationToken);
    }
}
using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Application.Interfaces.UseCases.Bookings
{
    public interface IGetBookingUseCase
    {
        Task<Booking> ExecuteAsync(Guid bookingId, CancellationToken cancellationToken);
    }
}
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Application.Interfaces.UseCases.Bookings
{
    public interface IUpdateBookingUseCase
    {
        Task<Booking> ExecuteAsync(Guid bookingId, UpdateBookingRequest request, CancellationToken cancellationToken);
    }
}

using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Application.Interfaces.UseCases.Bookings
{
    public interface IAddBookingUseCase
    {
        Task<Booking> ExecuteAsync(CreateBookingRequest request, string userId, CancellationToken cancellationToken);
    }
}
using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Application.Interfaces.UseCases.Bookings
{
    public interface IGetConfirmedBookingsByHousingIdUseCase
    {
        Task<IEnumerable<Booking>> ExecuteAsync(Guid housingId, CancellationToken cancellationToken);
    }
}

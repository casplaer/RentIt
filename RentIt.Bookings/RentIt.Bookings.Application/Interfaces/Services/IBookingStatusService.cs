namespace RentIt.Bookings.Application.Interfaces.Services
{
    internal interface IBookingStatusService
    {
        Task UpdateActiveBookings(CancellationToken cancellationToken);
    }
}

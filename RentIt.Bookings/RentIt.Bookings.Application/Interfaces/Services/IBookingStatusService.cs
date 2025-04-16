namespace RentIt.Bookings.Application.Interfaces.Services
{
    public interface IBookingStatusService
    {
        Task UpdateActiveBookingsAsync(CancellationToken cancellationToken);
        Task UpdatePaidBookingsAsync(CancellationToken cancellationToken);
        Task UpdateConfirmedBookingsAsync(CancellationToken cancellationToken);
        Task UpdatePendingBookingsAsync(CancellationToken cancellationToken);
    }
}
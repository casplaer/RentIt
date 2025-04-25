namespace RentIt.Bookings.Application.Interfaces.UseCases.Bookings
{
    public interface IConfirmBookingUseCase
    {
        Task ExecuteAsync(string userId, Guid bookingId, CancellationToken cancellationToken);
    }
}

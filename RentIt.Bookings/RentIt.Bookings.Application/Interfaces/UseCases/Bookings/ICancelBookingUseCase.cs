namespace RentIt.Bookings.Application.Interfaces.UseCases.Bookings
{
    public interface ICancelBookingUseCase
    {
        Task ExecuteAsync(Guid bookingId, string userId, CancellationToken cancellationToken);
    }
}

namespace RentIt.Bookings.Application.Interfaces.UseCases.Bookings
{
    public interface IRejectBookingUseCase
    {
        Task ExecuteAsync(
                string userId, 
                Guid bookingId, 
                CancellationToken cancellationToken);
    }
}

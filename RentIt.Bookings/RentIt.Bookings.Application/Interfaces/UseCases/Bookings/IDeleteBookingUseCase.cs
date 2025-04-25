namespace RentIt.Bookings.Application.Interfaces.UseCases.Bookings
{
    public interface IDeleteBookingUseCase
    {
        Task ExecuteAsync(Guid bookingId, CancellationToken cancellationToken);
    }
}

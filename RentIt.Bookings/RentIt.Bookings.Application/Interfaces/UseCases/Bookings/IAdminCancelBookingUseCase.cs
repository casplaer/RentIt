namespace RentIt.Bookings.Application.Interfaces.UseCases.Bookings
{
    public interface IAdminCancelBookingUseCase
    {
        Task ExecuteAsync(Guid bookingId, CancellationToken cancellationToken);
    }
}

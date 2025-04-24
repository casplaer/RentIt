namespace RentIt.Bookings.Application.Interfaces.UseCases.Bookings
{
    public interface ICheckIfBookingsExistUseCase
    {
        Task<bool> ExecuteAsync(Guid housingId, CancellationToken cancellationToken);
    }
}
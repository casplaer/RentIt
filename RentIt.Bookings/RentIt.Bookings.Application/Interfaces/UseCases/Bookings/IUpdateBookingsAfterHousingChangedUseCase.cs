using RentIt.MessageBroker.Contracts.Events;

namespace RentIt.Bookings.Application.Interfaces.UseCases.Bookings
{
    public interface IUpdateBookingsAfterHousingChangedUseCase
    {
        Task ExecuteAsync(HousingUpdatedEvent message, CancellationToken cancellationToken);
    }
}
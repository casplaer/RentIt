using RentIt.MessageBroker.Contracts.Events;
using MassTransit;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;

namespace RentIt.Bookings.Infrastructure.MessageBroker.Consumers
{
    public class HousingUpdatedEventConsumer : IConsumer<HousingUpdatedEvent>
    {
        private readonly IAppLogger _logger;
        private readonly IUpdateBookingsAfterHousingChangedUseCase _updateBookingsAfterHousingChangedUseCase;

        public HousingUpdatedEventConsumer(
            IAppLogger logger,
            IUpdateBookingsAfterHousingChangedUseCase updateBookingsAfterHousingChangedUseCase)
        {
            _logger = logger;
            _updateBookingsAfterHousingChangedUseCase = updateBookingsAfterHousingChangedUseCase;
        }

        public async Task Consume(ConsumeContext<HousingUpdatedEvent> context)
        {
            _logger.LogInformation("Сообщение об обновлении собственности успешно получено.");

            await _updateBookingsAfterHousingChangedUseCase.ExecuteAsync(context.Message, context.CancellationToken);
        }
    }
}
using MassTransit;
using RentIt.Housing.DataAccess.Enums;
using RentIt.Housing.Domain.Exceptions;
using RentIt.MessageBroker.Contracts.Events;
using Serilog;

namespace RentIt.Housing.Domain.Services.MessageBroker.Consumers
{
    public sealed class BookingCreatedEventConsumer : IConsumer<BookingCreatedEvent>
    {
        private readonly HousingService _housingService;
        private readonly ILogger _logger;

        public BookingCreatedEventConsumer(
            HousingService housingService,
            ILogger logger)
        {
            _housingService = housingService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
        {
            _logger.Information("Сообщение о создании бронирования успешно получено.");

            _logger.Information("Получение собственности с ID {HousingId}", context.Message.HousingId);
            var housing = await _housingService.GetByIdAsync(context.Message.HousingId, CancellationToken.None);

            if (housing == null)
            {
                _logger.Warning("Собственность с ID {HousingId} не найдена.", context.Message.HousingId);

                throw new NotFoundException("Собственность с таким ID не найдена.");
            }

            _logger.Information("Cобственность с ID {HousingId} успешно получена консьюмером. Обновляем собственность.", context.Message.HousingId);

            var newStartDate = DateOnly.FromDateTime(context.Message.StartDate);
            var newEndDate = DateOnly.FromDateTime(context.Message.EndDate);

            _logger.Information("Обновление примерной стартовой даты бронирования.");

            housing.Housing.EstimatedStartDate = newStartDate < housing.Housing.EstimatedStartDate ?
                newStartDate : housing.Housing.EstimatedStartDate;

            _logger.Information("Обновление примерной конечной даты бронирования.");

            housing.Housing.EstimatedEndDate = newEndDate > housing.Housing.EstimatedEndDate ? 
                newEndDate : housing.Housing.EstimatedEndDate;

            _logger.Information("Cобственность с ID {HousingId} успешно обновлена. Сохраняем изменения.", context.Message.HousingId);

            await _housingService.UpdateHousingAsync(housing.Housing, CancellationToken.None);
        }
    }
}
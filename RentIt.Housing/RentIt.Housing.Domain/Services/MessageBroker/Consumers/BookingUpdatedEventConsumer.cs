using MassTransit;
using RentIt.Housing.DataAccess.Enums;
using RentIt.Housing.Domain.Exceptions;
using RentIt.MessageBroker.Contracts.Events;
using Serilog;

namespace RentIt.Housing.Domain.Services.MessageBroker.Consumers
{
    public sealed class BookingUpdatedEventConsumer : IConsumer<BookingUpdatedEvent>
    {
        private readonly HousingService _housingService;
        private readonly ILogger _logger;

        public BookingUpdatedEventConsumer(
            HousingService housingService,
            ILogger logger)
        {
            _housingService = housingService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingUpdatedEvent> context)
        {
            _logger.Information("Сообщение об отмене бронирования успешно получено.");

            _logger.Information("Получение собственности с ID {HousingId}", context.Message.HousingId);

            var housing = await _housingService.GetByIdAsync(context.Message.HousingId, CancellationToken.None);

            if (housing == null)
            {
                _logger.Warning("Собственность с ID {HousingId} не найдена.", context.Message.HousingId);

                throw new NotFoundException("Собственность с таким ID не найдена.");
            }

            _logger.Information("Cобственность с ID {HousingId} успешно получена консьюмером. Обновляем собственность.", context.Message.HousingId);

            housing.Housing.EstimatedStartDate = context.Message.NewStartDate;
            housing.Housing.EstimatedEndDate = context.Message.NewEndDate;

            if (housing.Housing.EstimatedStartDate == null || (housing.Housing.EstimatedStartDate - DateTime.UtcNow).Value.TotalHours > 36)
            {
                housing.Housing.Status = HousingStatus.Available;
            }
            else if (context.Message.BookingStatus == "Active")
            {
                housing.Housing.Status = HousingStatus.Booked;
            }

            _logger.Information("Cобственность с ID {HousingId} успешно обновлена. Сохраняем изменения.", context.Message.HousingId);

            await _housingService.UpdateHousingAsync(housing.Housing, CancellationToken.None);
        }
    }
}
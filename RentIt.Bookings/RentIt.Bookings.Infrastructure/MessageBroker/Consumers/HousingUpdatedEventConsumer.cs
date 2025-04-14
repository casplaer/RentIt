using RentIt.MessageBroker.Contracts.Events;
using MassTransit;
using Serilog;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Core.Interfaces.Repositories;

namespace RentIt.Bookings.Infrastructure.MessageBroker.Consumers
{
    public class HousingUpdatedEventConsumer : IConsumer<HousingUpdatedEvent>
    {
        private readonly ILogger _logger;
        private readonly IGetConfirmedBookingsByHousingIdUseCase _getConfirmedBookingsByHousingIdUseCase;
        private readonly IUnitOfWork _unitOfWork;

        public HousingUpdatedEventConsumer(
            ILogger logger,
            IGetConfirmedBookingsByHousingIdUseCase getConfirmedBookingsByHousingIdUseCase,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _getConfirmedBookingsByHousingIdUseCase = getConfirmedBookingsByHousingIdUseCase;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<HousingUpdatedEvent> context)
        {
            _logger.Information("Сообщение об обновлении собственности успешно получено.");

            _logger.Information("Получение всех бронирований по собственности с ID {HousingId}.", context.Message.HousingId);

            var bookingsToUpdate = await _getConfirmedBookingsByHousingIdUseCase.ExecuteAsync(context.Message.HousingId, CancellationToken.None); 

            foreach (var booking in bookingsToUpdate)
            {
                _logger.Information("Подсчет количества ночей для каждого бронирования.");

                var nights = (booking.EndDate.Date - booking.StartDate.Date).Days;

                _logger.Information("Высчет новой цены для каждого бронирования.");

                var newPrice = nights * context.Message.NewPricePerNight;

                booking.TotalPrice = newPrice;
                booking.UpdatedAt = DateTime.UtcNow;

                _logger.Information("Сохранение изменений в бронировании с ID {BookingId}.", booking.BookingId);

                _unitOfWork.Bookings.Update(booking);
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            }
        }
    }
}

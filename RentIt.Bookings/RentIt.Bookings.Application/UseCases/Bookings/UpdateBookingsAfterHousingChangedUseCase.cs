using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.MessageBroker.Contracts.Events;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class UpdateBookingsAfterHousingChangedUseCase : IUpdateBookingsAfterHousingChangedUseCase
    {
        private readonly IGetConfirmedBookingsByHousingIdUseCase _getConfirmedBookingsByHousingIdUseCase;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public UpdateBookingsAfterHousingChangedUseCase(
            IGetConfirmedBookingsByHousingIdUseCase getConfirmedBookingsByHousingIdUseCase,
            IUnitOfWork unitOfWork,
            ILogger logger)
        {
            _getConfirmedBookingsByHousingIdUseCase = getConfirmedBookingsByHousingIdUseCase;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ExecuteAsync(HousingUpdatedEvent message, CancellationToken cancellationToken)
        {
            _logger.Information("Получение всех бронирований для обновления.");

            var bookingsToUpdate = await _getConfirmedBookingsByHousingIdUseCase.ExecuteAsync(message.HousingId, cancellationToken);

            foreach (var booking in bookingsToUpdate)
            {
                var nights = (booking.EndDate.Date - booking.StartDate.Date).Days;
                var newPrice = nights * message.NewPricePerNight;

                booking.TotalPrice = newPrice;
                booking.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Bookings.Update(booking);
            }

            _logger.Information("Бронирования успешно обновлены. Сохраняем изменения.");

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
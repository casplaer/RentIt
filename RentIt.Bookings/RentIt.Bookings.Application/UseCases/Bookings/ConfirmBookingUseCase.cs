using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.EventBus;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using RentIt.Bookings.Application.Services.Grpc;
using RentIt.Bookings.Contracts.Requests.Payments;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.MessageBroker.Contracts.Events;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class ConfirmBookingUseCase : IConfirmBookingUseCase
    {
        private readonly ILogger _logger;
        private readonly HousingIntegrationService _housingIntegrationsService;
        private readonly ICreatePaymentUseCase _createPaymentUseCase;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;

        public ConfirmBookingUseCase(
            ILogger logger,
            HousingIntegrationService housingIntegrationsService,
            ICreatePaymentUseCase createPaymentUseCase,
            IUnitOfWork unitOfWork,
            IEventBus eventBus)
        {
            _logger = logger;
            _housingIntegrationsService = housingIntegrationsService;
            _createPaymentUseCase = createPaymentUseCase;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;

        }

        public async Task ExecuteAsync(string userId, Guid bookingId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.Warning("Некорректный формат UserId: {UserId}", userId);

                throw new ArgumentException("Некорректный формат ID.");
            }

            _logger.Information("Проверка прав пользователя на доступ к данному бронированию");

            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                _logger.Warning("Бронирование с ID {BookingId} не найдено.", bookingId);

                throw new NotFoundException("Бронирование не найдено.");
            }

            var housingResponse = await _housingIntegrationsService.GetHousingInfoAsync(booking.HousingId);

            if (userGuid != housingResponse.OwnerId)
            {
                _logger.Warning("Попытка неавторизованного доступа к бронированию.");

                throw new ArgumentException("Попытка неавторизованного доступа к бронированию.");
            }

            _logger.Information("Изменение статуса бронирования на \"Подтверждено\".");
            
            booking.Status = BookingStatus.Confirmed;

            _logger.Information("Публикация сообщения об успешном создании бронирования в брокер сообщений.");

            await _createPaymentUseCase.ExecuteAsync(new ProcessTestPaymentRequest(bookingId, booking.TotalPrice), cancellationToken);

            await _eventBus.PublishAsync(
                new BookingConfirmedEvent
                {
                    HousingId = booking.HousingId,
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate,
                }, cancellationToken);

            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
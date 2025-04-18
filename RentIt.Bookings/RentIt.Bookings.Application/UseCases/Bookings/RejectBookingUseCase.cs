using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Services;
using RentIt.Bookings.Application.Services.Grpc;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class RejectBookingUseCase : IRejectBookingUseCase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly HousingIntegrationService _housingIntegrationService;
        private readonly BookingNotificationService _bookingNotificationService;

        public RejectBookingUseCase(
            ILogger logger,
            IUnitOfWork unitOfWork,
            HousingIntegrationService housingIntegrationService,
            BookingNotificationService bookingNotificationService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _housingIntegrationService = housingIntegrationService;
            _bookingNotificationService = bookingNotificationService;
        }

        public async Task ExecuteAsync(string userId, Guid bookingId, CancellationToken cancellationToken)
        {
            _logger.Information("Начало обновление статуса бронирования {BookingId} на Rejected.", bookingId);

            var userIdParseAttempt = Guid.TryParse(userId, out var userGuid);

            if (!userIdParseAttempt)
            {
                _logger.Warning("Некорректный формат ID пользователя.");

                throw new ArgumentException("Некорректный формат ID пользователя.");
            }

            var bookingToReject = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);

            if (bookingToReject == null)
            {
                _logger.Warning("Бронирование с ID {BookingId} не было найдено.", bookingId);

                throw new NotFoundException("Бронирование с таким ID не найдено.");
            }

            var housing = await _housingIntegrationService.GetHousingInfoAsync(bookingToReject.HousingId);

            if (housing.OwnerId != userGuid)
            {
                _logger.Warning("Произошла попытка неавторизованного доступа пользователя {UserId} к бронированию {BookingId}.", userId, bookingId);

                throw new UnauthorizedAccessException("Попытка неавторизованного доступа.");
            }

            bookingToReject.Status = BookingStatus.Rejected;

            _logger.Information("Статус бронирования успешно изменен на Rejected.");

            _unitOfWork.Bookings.Update(bookingToReject);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.Information("Изменения успешно сохранены.");

            await _bookingNotificationService.NotifyUserAboutBookingRejectionAsync(bookingToReject, housing, cancellationToken);
        }
    }
}

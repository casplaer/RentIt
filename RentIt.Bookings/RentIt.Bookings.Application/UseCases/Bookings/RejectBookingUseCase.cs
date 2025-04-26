using Hangfire;
using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.Services.Grpc;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Application.Interfaces.Services;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class RejectBookingUseCase : IRejectBookingUseCase
    {
        private readonly IAppLogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHousingIntegrationService _housingIntegrationService;
        private readonly IBookingNotificationService _bookingNotificationService;

        public RejectBookingUseCase(
            IAppLogger logger,
            IUnitOfWork unitOfWork,
            IHousingIntegrationService housingIntegrationService,
            IBookingNotificationService bookingNotificationService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _housingIntegrationService = housingIntegrationService;
            _bookingNotificationService = bookingNotificationService;
        }

        public async Task ExecuteAsync(string userId, Guid bookingId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обновление статуса бронирования {BookingId} на Rejected.", bookingId);

            var userIdParseAttempt = Guid.TryParse(userId, out var userGuid);

            if (!userIdParseAttempt)
            {
                _logger.LogWarning("Некорректный формат ID пользователя.");

                throw new ArgumentException("Некорректный формат ID пользователя.");
            }

            var bookingToReject = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);

            if (bookingToReject == null)
            {
                _logger.LogWarning("Бронирование с ID {BookingId} не было найдено.", bookingId);

                throw new NotFoundException("Бронирование с таким ID не найдено.");
            }

            var housing = await _housingIntegrationService.GetHousingInfoAsync(bookingToReject.HousingId);

            if (housing.OwnerId != userGuid)
            {
                _logger.LogWarning("Произошла попытка неавторизованного доступа пользователя {UserId} к бронированию {BookingId}.", userId, bookingId);

                throw new UnauthorizedAccessException("Попытка неавторизованного доступа.");
            }

            bookingToReject.Status = BookingStatus.Rejected;

            _logger.LogInformation("Статус бронирования успешно изменен на Rejected.");

            _unitOfWork.Bookings.Update(bookingToReject);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Изменения успешно сохранены.");

            BackgroundJob.Enqueue(() =>
                _bookingNotificationService.NotifyUserAboutBookingRejectionAsync(bookingToReject, housing, cancellationToken));
        }
    }
}
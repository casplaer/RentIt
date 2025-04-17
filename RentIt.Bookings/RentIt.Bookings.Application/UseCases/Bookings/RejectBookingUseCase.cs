using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
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
        private readonly HousingIntegrationsService _housingIntegrationService;
        private readonly UserIntegrationService _userIntegrationService;
        private readonly IEmailSender _emailSender;

        public RejectBookingUseCase(
            ILogger logger,
            IUnitOfWork unitOfWork,
            HousingIntegrationsService housingIntegrationService,
            UserIntegrationService userIntegrationService,
            IEmailSender emailSender)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _housingIntegrationService = housingIntegrationService;
            _userIntegrationService = userIntegrationService;
            _emailSender = emailSender;
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

            var userInfo = await _userIntegrationService.GetUserInfoAsync(bookingToReject.UserId);

            if (userInfo == null)
            {
                _logger.Warning("Не удалось получить информацию о пользователе с ID {UserId} для отправки email", bookingToReject.UserId);
            }
            else
            {
                var subject = "Бронирование отклонено.";
                var body = $"Здравствуйте, {userInfo.FirstName}! Ваша заявка на бронирование {housing.HousingName}" +
                    $" от {bookingToReject.StartDate:dd.MM.yyyy} до {bookingToReject.EndDate:dd.MM.yyyy} была отклонена владельцем.\n\n" +
                    "С уважением,\nКоманда RentIt."; ;

                await _emailSender.SendEmailAsync(userInfo.Email, subject, body, cancellationToken);
                _logger.Information("Уведомление по email отправлено пользователю {UserEmail}", userInfo.Email);
            }
        }
    }
}

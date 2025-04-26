using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.Services.Grpc;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Application.Interfaces.Services;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class GetBookingUseCase : IGetBookingUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppLogger _logger;
        private readonly IHousingIntegrationService _housingIntegrationsService;

        public GetBookingUseCase(
            IUnitOfWork unitOfWork, 
            IAppLogger logger,
            IHousingIntegrationService housingIntegrationsService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _housingIntegrationsService = housingIntegrationsService;
        }

        public async Task<Booking> ExecuteAsync(
            Guid bookingId,
            string authenticatedUserId,
            string authenticatedUserRole,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Получение бронирования с BookingId: {BookingId}", bookingId);

            var authenticatedUserIdParseAttmept = Guid.TryParse(authenticatedUserId, out var authenticatedUserGuid);

            if (!authenticatedUserIdParseAttmept)
            {
                _logger.LogWarning("Некорректный формат UserId: {UserId}", authenticatedUserId);

                throw new ArgumentException("Некорректный формат ID.");
            }

            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                _logger.LogWarning("Бронирование с BookingId {BookingId} не найдено", bookingId);

                throw new NotFoundException("Бронирование с таким ID не найдено.");
            }

            var housing = await _housingIntegrationsService.GetHousingInfoAsync(booking.HousingId);

            if (authenticatedUserGuid != booking.UserId && authenticatedUserGuid != housing.OwnerId && authenticatedUserRole != "Admin")
            {
                _logger.LogWarning("Произошла попытка неавторизованного доступа к данным о бронировании пользователя {UserId}.", booking.UserId);

                throw new UnauthorizedAccessException("Попытка неавторизованного доступа.");
            }

            _logger.LogInformation("Бронирование с BookingId {BookingId} успешно получено", bookingId);

            return booking;
        }
    }
}

using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Services.Grpc;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class GetBookingUseCase : IGetBookingUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly HousingIntegrationService _housingIntegrationsService;

        public GetBookingUseCase(
            IUnitOfWork unitOfWork, 
            ILogger logger,
            HousingIntegrationService housingIntegrationsService)
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
            _logger.Information("Получение бронирования с BookingId: {BookingId}", bookingId);

            var authenticatedUserIdParseAttmept = Guid.TryParse(authenticatedUserId, out var authenticatedUserGuid);

            if (!authenticatedUserIdParseAttmept)
            {
                _logger.Warning("Некорректный формат UserId: {UserId}", authenticatedUserId);

                throw new ArgumentException("Некорректный формат ID.");
            }

            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                _logger.Warning("Бронирование с BookingId {BookingId} не найдено", bookingId);

                throw new NotFoundException("Бронирование с таким ID не найдено.");
            }

            var housing = await _housingIntegrationsService.GetHousingInfoAsync(booking.HousingId);

            if (authenticatedUserGuid != booking.UserId && authenticatedUserGuid != housing.OwnerId && authenticatedUserRole != "Admin")
            {
                _logger.Warning("Произошла попытка неавторизованного доступа к данным о бронировании пользователя {UserId}.", booking.UserId);

                throw new UnauthorizedAccessException("Попытка неавторизованного доступа.");
            }

            _logger.Information("Бронирование с BookingId {BookingId} успешно получено", bookingId);

            return booking;
        }
    }
}

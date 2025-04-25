using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Specifications.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class GetConfirmedBookingsByHousingIdUseCase : IGetConfirmedBookingsByHousingIdUseCase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public GetConfirmedBookingsByHousingIdUseCase(
            ILogger logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Booking>> ExecuteAsync(Guid housingId, CancellationToken cancellationToken)
        {
            _logger.Information("Получение всех подтвержденных бронирований с ID собственности {HousingId}.", housingId);

            var specification = new SearchBookingSpecification(
                                        housingId: housingId,
                                        status: BookingStatus.Confirmed);

            var confirmedBookings = await _unitOfWork.Bookings.GetAllFilteredBookingsAsync(specification, cancellationToken);

            _logger.Information("Список подтвержденных бронирований с ID собственности {HousingId} успешно получен.", housingId);

            return confirmedBookings;
        }
    }
}

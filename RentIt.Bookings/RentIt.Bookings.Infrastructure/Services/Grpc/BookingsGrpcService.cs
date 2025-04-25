using Grpc.Core;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Specifications.Bookings;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Protos.Booking;
using Serilog;

namespace RentIt.Bookings.Infrastructure.Services.Grpc
{
    public class BookingsGrpcService : BookingService.BookingServiceBase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public BookingsGrpcService(
            ILogger logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public override async Task<GetExistBookingsResponse> GetBookings(GetExistBookingsRequest request, ServerCallContext context)
        {
            _logger.Information("Проверка на существование бронирований на собственность с ID {HousingId}.", request.HousingId);

            var housingIdParseAttempt = Guid.TryParse(request.HousingId, out var housingGuid);

            if (!housingIdParseAttempt)
            {
                _logger.Warning("Неверный формат housing_id. Ожидается GUID.");

                throw new RpcException(new Status(StatusCode.InvalidArgument, "Неверный формат housing_id. Ожидается GUID."));
            }

            List<BookingStatus> notAllowedToDeleteStatuses =
            [
                BookingStatus.Pending,
                BookingStatus.Confirmed,
                BookingStatus.Active,
                BookingStatus.Paid,
            ];

            var bookings = await _unitOfWork.Bookings.GetBookingsByStatusesAsync(housingGuid, notAllowedToDeleteStatuses, CancellationToken.None);

            if (bookings == null)
            {
                return new GetExistBookingsResponse
                {
                    BookingsExist = false,
                };
            }

            return new GetExistBookingsResponse
            {
                BookingsExist = true,
            };
        }
    }
}
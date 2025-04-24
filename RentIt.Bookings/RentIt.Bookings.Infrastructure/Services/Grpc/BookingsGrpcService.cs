using Grpc.Core;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Protos.Booking;
using Serilog;

namespace RentIt.Bookings.Infrastructure.Services.Grpc
{
    public class BookingsGrpcService : BookingService.BookingServiceBase
    {
        private readonly ILogger _logger;
        private readonly ICheckIfBookingsExistUseCase _checkIfBookingsExistUseCase;

        public BookingsGrpcService(
            ILogger logger,
            ICheckIfBookingsExistUseCase checkIfBookingsExistUseCase)
        {
            _logger = logger;
            _checkIfBookingsExistUseCase = checkIfBookingsExistUseCase;
        }

        public override async Task<GetExistBookingsResponse> GetBookings(GetExistBookingsRequest request, ServerCallContext context)
        {
            _logger.Information("Проверка на существование бронирований на собственность с ID {HousingId}.", request.HousingId);

            if (!Guid.TryParse(request.HousingId, out var housingGuid))
            {
                _logger.Warning("Неверный формат housing_id. Ожидается GUID.");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Неверный формат housing_id. Ожидается GUID."));
            }

            var bookingsExist = await _checkIfBookingsExistUseCase.ExecuteAsync(housingGuid, context.CancellationToken);

            return new GetExistBookingsResponse
            {
                BookingsExist = bookingsExist
            };
        }
    }
}
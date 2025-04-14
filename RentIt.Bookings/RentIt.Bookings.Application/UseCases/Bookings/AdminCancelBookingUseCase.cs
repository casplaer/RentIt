using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class AdminCancelBookingUseCase : IAdminCancelBookingUseCase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public AdminCancelBookingUseCase(
            ILogger logger, 
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public Task ExecuteAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            _logger.Information("Начало отмены бронирования с ID {BookingId} администратором.", bookingId);



            throw new NotImplementedException();
        }
    }
}

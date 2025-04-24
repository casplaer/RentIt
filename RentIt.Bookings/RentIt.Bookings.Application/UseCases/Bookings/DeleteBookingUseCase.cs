using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class DeleteBookingUseCase : IDeleteBookingUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public DeleteBookingUseCase(IUnitOfWork unitOfWork, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            _logger.Information("Запрос на удаление бронирования с Id: {BookingId}", bookingId);

            var bookingToDelete = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);

            if (bookingToDelete == null)
            {
                _logger.Warning("Бронирование с Id: {BookingId} не найдено", bookingId);

                throw new NotFoundException("Бронирование с таким ID не найдено.");
            }

            _unitOfWork.Bookings.Delete(bookingToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.Information("Бронирование с Id: {BookingId} успешно удалено", bookingId);
        }
    }
}
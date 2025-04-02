using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class GetBookingUseCase : IGetBookingUseCase
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetBookingUseCase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Booking> ExecuteAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                throw new NotFoundException("Бронирование с таким ID не найдено.");
            }

            return booking;
        }
    }
}

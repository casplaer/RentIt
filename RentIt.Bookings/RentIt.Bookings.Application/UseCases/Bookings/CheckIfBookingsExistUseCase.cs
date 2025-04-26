using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class CheckIfBookingsExistUseCase : ICheckIfBookingsExistUseCase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckIfBookingsExistUseCase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> ExecuteAsync(Guid housingId, CancellationToken cancellationToken)
        {
            List<BookingStatus> notAllowedToDeleteStatuses =
            [
                BookingStatus.Pending,
                BookingStatus.Confirmed,
                BookingStatus.Active,
                BookingStatus.Paid,
            ];

            var bookings = await _unitOfWork.Bookings.GetBookingsByStatusesAsync(housingId, notAllowedToDeleteStatuses, cancellationToken);

            return bookings != null && bookings.Any();
        }
    }
}
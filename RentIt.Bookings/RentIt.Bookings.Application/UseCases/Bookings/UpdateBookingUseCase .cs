using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class UpdateBookingUseCase : IUpdateBookingUseCase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBookingUseCase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Booking> ExecuteAsync(Guid bookingId, UpdateBookingRequest request, CancellationToken cancellationToken)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                throw new Exception("Бронирование не найдено");
            }    

            booking.HousingId = request.HousingId;
            booking.StartDate = request.StartDate;
            booking.EndDate = request.EndDate;
            booking.TotalPrice = request.TotalPrice;
            booking.Status = request.Status;
            booking.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync();

            return booking;
        }
    }
}
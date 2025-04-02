using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Core.Interfaces.Repositories;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class DeleteBookingUseCase : IDeleteBookingUseCase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBookingUseCase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            var bookingToDelete = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);

            if(bookingToDelete == null)
            {
                throw new NotFoundException("Бронирование с таким ID не найдено.");
            }

            _unitOfWork.Bookings.Delete(bookingToDelete);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

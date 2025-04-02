using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Services.Grpc;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Protos.Housing;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class AddBookingUseCase : IAddBookingUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HousingIntegrationsService _housingService;

        public AddBookingUseCase(IUnitOfWork unitOfWork, HousingIntegrationsService housingService)
        {
            _unitOfWork = unitOfWork;
            _housingService = housingService;
        }

        public async Task<Booking> ExecuteAsync(
            CreateBookingRequest request,
            string userId,
            CancellationToken cancellationToken)
        {
            if(!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Некорректный формат ID.");
            }

            var housingResponse = await _housingService.GetHousingInfoAsync(userGuid);

            int nights = (request.EndDate.Date - request.StartDate.Date).Days;
            if (nights <= 0)
            {
                throw new Exception("Неверный период бронирования");
            }

            decimal computedTotalPrice = (decimal)housingResponse.PricePerNight * nights;

            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                HousingId = request.HousingId,
                UserId = userGuid,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalPrice = computedTotalPrice,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Bookings.AddAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync();

            return booking;
        }
    }
}

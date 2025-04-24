using RentIt.Housing.Domain.Services.Interfaces;
using RentIt.Protos.Booking;

namespace RentIt.Housing.Domain.Services.Grpc
{
    public class BookingIntegrationService : IBookingIntegrationService
    {
        private readonly BookingService.BookingServiceClient _bookingsClient;

        public BookingIntegrationService(BookingService.BookingServiceClient bookingsClient)
        {
            _bookingsClient = bookingsClient;
        }

        public async Task<bool> GetExistBookings(Guid housingId)
        {
            var request = new GetExistBookingsRequest { HousingId = housingId.ToString() };

            var response = await _bookingsClient.GetBookingsAsync(request);

            return response.BookingsExist;
        }
    }
}

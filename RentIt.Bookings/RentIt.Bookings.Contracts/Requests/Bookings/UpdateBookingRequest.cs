using RentIt.Bookings.Core.Enums;

namespace RentIt.Bookings.Contracts.Requests.Bookings
{
    public record UpdateBookingRequest(
          Guid HousingId,
          DateTime StartDate,
          DateTime EndDate,
          decimal TotalPrice,
          BookingStatus Status
      );
}

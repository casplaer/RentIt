namespace RentIt.Bookings.Contracts.Requests.Bookings
{
    public record CreateBookingRequest(
        Guid HousingId,
        DateTime StartDate,
        DateTime EndDate
        );
}

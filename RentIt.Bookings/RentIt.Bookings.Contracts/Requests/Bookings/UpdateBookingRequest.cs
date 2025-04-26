namespace RentIt.Bookings.Contracts.Requests.Bookings
{
    public record UpdateBookingRequest(
        Guid HousingId,
        DateTime StartDate,
        DateTime EndDate,
        string Status
    );
}

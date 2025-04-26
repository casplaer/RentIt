namespace RentIt.Bookings.Contracts.Requests.Bookings
{
    public record GetBookingsByPagesRequest(
        int page,
        int pageSize
        );
}

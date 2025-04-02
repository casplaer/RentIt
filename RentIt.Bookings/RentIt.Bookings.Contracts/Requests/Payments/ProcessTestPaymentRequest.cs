namespace RentIt.Bookings.Contracts.Requests.Payments
{
    public record ProcessTestPaymentRequest(
        Guid BookingId,
        decimal Amount
    );
}

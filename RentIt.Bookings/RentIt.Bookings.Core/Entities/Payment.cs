using RentIt.Bookings.Core.Enums;

namespace RentIt.Bookings.Core.Entities
{
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentTime { get; set; }
        public PaymentStatus Status { get; set; }
        public Booking Booking { get; set; }
    }
}

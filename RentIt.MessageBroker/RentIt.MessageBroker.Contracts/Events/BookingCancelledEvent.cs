namespace RentIt.MessageBroker.Contracts.Events
{
    public record BookingCancelledEvent
    {
        public Guid HousingId { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public DateTime? NextEstimatedStartDate { get; init; }
        public DateTime? NextEstimatedEndDate { get; init; }
    }
}
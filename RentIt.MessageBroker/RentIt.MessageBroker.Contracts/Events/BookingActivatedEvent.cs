namespace RentIt.MessageBroker.Contracts.Events
{
    public record BookingActivatedEvent
    {
        public Guid HousingId { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
}
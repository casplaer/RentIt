namespace RentIt.MessageBroker.Contracts.Events
{
    public record BookingUpdatedEvent
    {
        public Guid HousingId { get; init; }
        public DateTime? NewStartDate { get; init; }
        public DateTime? NewEndDate { get; init; }
        public string? BookingStatus { get; init; }
    }
}
namespace RentIt.MessageBroker.Contracts.Events
{
    public record HousingUpdatedEvent
    {
        public Guid HousingId { get; init; }
        public decimal NewPricePerNight { get; init; }
    }
}

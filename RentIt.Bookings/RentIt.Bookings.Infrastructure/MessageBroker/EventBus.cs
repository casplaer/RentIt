using MassTransit;
using RentIt.Bookings.Application.Interfaces.EventBus;

namespace RentIt.Bookings.Infrastructure.MessageBroker
{
    public sealed class EventBus : IEventBus
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public EventBus(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
            where T : class
        {
            return _publishEndpoint.Publish(message, cancellationToken);
        }
    }
}

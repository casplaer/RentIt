using MassTransit;
using RentIt.Housing.Domain.Services.Interfaces;

namespace RentIt.Housing.Domain.Services.MessageBroker
{
    public class EventBus : IEventBus
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

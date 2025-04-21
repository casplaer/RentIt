using MassTransit;
using RentIt.MessageBroker.Contracts.Events;
using Serilog;

namespace RentIt.Housing.Domain.Services.MessageBroker.Consumers
{
    public class BookingActivatedEventConsumer : IConsumer<BookingActivatedEvent>
    {
        private readonly HousingService _housingService;
        private readonly ILogger _logger;

        public BookingActivatedEventConsumer(
            HousingService housingService, 
            ILogger logger)
        {
            _housingService = housingService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingActivatedEvent> context)
        {

        }
    }
}

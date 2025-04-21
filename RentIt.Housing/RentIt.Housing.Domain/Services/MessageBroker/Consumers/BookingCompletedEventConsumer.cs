using MassTransit;
using RentIt.MessageBroker.Contracts.Events;
using Serilog;

namespace RentIt.Housing.Domain.Services.MessageBroker.Consumers
{
    public class BookingCompletedEventConsumer : IConsumer<BookingCompletedEvent>
    {
        private readonly HousingService _housingService;
        private readonly ILogger _logger;

        public BookingCompletedEventConsumer(
            HousingService housingService, 
            ILogger logger)
        {
            _housingService = housingService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingCompletedEvent> context)
        {

        }
    }
}

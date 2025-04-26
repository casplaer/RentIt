using RentIt.Bookings.Application.Interfaces.Services;
using Serilog;

namespace RentIt.Bookings.Application.Services
{
    public class AppLogger : IAppLogger
    {
        private readonly ILogger _logger;

        public AppLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void LogInformation(string messageTemplate, params object[] propertyValues)
        {
            _logger.Information(messageTemplate, propertyValues);
        }

        public void LogWarning(string messageTemplate, params object[] propertyValues)
        {
            _logger.Warning(messageTemplate, propertyValues);
        }

        public void LogError(string messageTemplate, params object[] propertyValues)
        {
            _logger.Error(messageTemplate, propertyValues);
        }
    }
}
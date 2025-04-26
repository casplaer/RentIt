namespace RentIt.Bookings.Application.Interfaces.Services
{
    public interface IAppLogger
    {
        void LogInformation(string messageTemplate, params object[] propertyValues);
        void LogWarning(string messageTemplate, params object[] propertyValues);
        void LogError(string messageTemplate, params object[] propertyValues);
    }
}

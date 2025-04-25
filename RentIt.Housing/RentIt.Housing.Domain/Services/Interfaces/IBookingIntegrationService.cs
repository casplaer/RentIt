namespace RentIt.Housing.Domain.Services.Interfaces
{
    public interface IBookingIntegrationService
    {
        Task<bool> GetExistBookings(Guid housingId);
    }
}
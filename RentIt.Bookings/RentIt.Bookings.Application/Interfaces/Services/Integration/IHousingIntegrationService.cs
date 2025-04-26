using RentIt.Bookings.Contracts.Dto;

namespace RentIt.Bookings.Application.Interfaces.Services.Grpc
{
    public interface IHousingIntegrationService
    {
        Task<HousingInfoDto> GetHousingInfoAsync(Guid housingId);
    }
}
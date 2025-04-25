using RentIt.Bookings.Contracts.Dto;
using RentIt.Protos.Housing;

namespace RentIt.Bookings.Application.Services.Grpc
{
    public class HousingIntegrationsService
    {
        private readonly HousingService.HousingServiceClient _housingServiceClient;

        public HousingIntegrationsService(HousingService.HousingServiceClient housingServiceClient)
        {
            _housingServiceClient = housingServiceClient;
        }

        public async Task<HousingInfoDto> GetHousingInfoAsync(Guid housingId)
        {
            var request = new GetHousingRequest { HousingId = housingId.ToString() };

            var response = await _housingServiceClient.GetHousingAsync(request);

            return new HousingInfoDto
            (
                Guid.Parse(response.OwnerId),
                response.HousingName,
                (decimal) response.PricePerNight
            );
        }
    }
}

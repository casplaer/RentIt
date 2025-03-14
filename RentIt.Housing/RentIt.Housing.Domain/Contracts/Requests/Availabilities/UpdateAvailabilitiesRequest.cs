using RentIt.Availabilities.Domain.Contracts.Dto.Availabilities;

namespace RentIt.Housing.Domain.Contracts.Requests.Availabilities
{
    public record UpdateAvailabilitiesRequest(
        IEnumerable<AvailabilityDto> AvailabilityDtos
        );
}

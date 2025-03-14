using RentIt.Availabilities.Domain.Contracts.Dto.Availabilities;

namespace RentIt.Housing.Domain.Contracts.Requests.Availabilities
{
    public record CreateAvailabilitiesRequest(
        IEnumerable<AvailabilityDto> AvailabilityDtos
        );
}

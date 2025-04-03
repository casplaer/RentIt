using RentIt.Housing.Domain.Contracts.Dto.Housing;

namespace RentIt.Housing.Domain.Contracts.Responses.Housing
{
    public record GetAllHousingsResponse(
        IEnumerable<HousingDto> Housing
        );
}

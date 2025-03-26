using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.Domain.Contracts.Dto.Users;

namespace RentIt.Housing.Domain.Contracts.Responses.Housing
{
    public record GetHousingByIdResponse(
        HousingEntity Housing,
        UserInfoDto UserInfoDto
        );
}

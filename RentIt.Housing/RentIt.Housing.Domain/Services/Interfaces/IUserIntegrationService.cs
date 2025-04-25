using RentIt.Housing.Domain.Contracts.Dto.Users;

namespace RentIt.Housing.Domain.Services.Interfaces
{
    public interface IUserIntegrationService
    {
        Task<UserInfoDto> GetUserInfoAsync(Guid ownerId);
    }

}

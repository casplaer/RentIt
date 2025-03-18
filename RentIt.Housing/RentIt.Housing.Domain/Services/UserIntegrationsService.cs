using RentIt.Housing.Domain.Contracts.Dto.Users;
using RentIt.Protos.Users;

namespace RentIt.Housing.Domain.Services
{
    public class UserIntegrationService
    {
        private readonly UsersService.UsersServiceClient _usersClient;

        public UserIntegrationService(UsersService.UsersServiceClient usersClient)
        {
            _usersClient = usersClient;
        }

        public async Task<UserInfoDto> GetOwnerInfoAsync(Guid ownerId)
        {
            var request = new GetUserRequest { UserId = ownerId.ToString() };

            var response = await _usersClient.GetUserAsync(request);

            return new UserInfoDto(
                response.FirstName, 
                response.LastName,
                response.Email,
                response.PhoneNumber);
        }
    }
}
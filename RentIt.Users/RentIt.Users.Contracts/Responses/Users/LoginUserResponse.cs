using RentIt.Users.Contracts.Dto.Users;

namespace RentIt.Users.Contracts.Responses.Users
{
    public record LoginUserResponse(string AccessToken, string RefreshToken, UserDto User);
}

using RentIt.Users.Contracts.DTO.Users;

namespace RentIt.Users.Contracts.Responses.Users
{
    public record LoginUserResponse(string AccessToken, string RefreshToken, UserDTO User);
}

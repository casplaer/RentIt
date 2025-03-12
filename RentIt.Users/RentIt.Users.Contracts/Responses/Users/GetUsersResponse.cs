using RentIt.Users.Contracts.Dto.Users;

namespace RentIt.Users.Contracts.Responses.Users
{
    public record GetUsersResponse(
        ICollection<UserDto> Users,
        int PageNumber,
        int TotalPages
        );
}

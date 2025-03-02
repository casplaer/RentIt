using RentIt.Users.Contracts.DTO.Users;

namespace RentIt.Users.Contracts.Responses.Users
{
    public record GetUsersResponse(
        ICollection<UserDTO> Users,
        int PageNumber,
        int TotalPages
        );
}

using RentIt.Users.Contracts.DTO.Users;

namespace RentIt.Users.Contracts.Responses.Users
{
    public record GetEventsResponse(
        ICollection<UserDTO> Users,
        int PageNumber,
        int TotalPages
        );
}

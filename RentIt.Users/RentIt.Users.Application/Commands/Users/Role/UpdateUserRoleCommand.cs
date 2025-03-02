using MediatR;

namespace RentIt.Users.Application.Commands.Users.Role
{
    public record UpdateUserRoleCommand(
        Guid UserId, 
        string NewRole) : IRequest<bool>;
}

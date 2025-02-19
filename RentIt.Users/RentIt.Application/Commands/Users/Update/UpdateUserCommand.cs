using MediatR;

namespace RentIt.Users.Application.Commands.Users.Update
{
    public record UpdateUserCommand(
        Guid UserId,
        string FirstName,
        string LastName,
        string Email
        ) : IRequest<bool>;
}

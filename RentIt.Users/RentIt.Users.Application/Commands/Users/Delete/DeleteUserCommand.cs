using MediatR;

namespace RentIt.Users.Application.Commands.Users.Delete
{
    public record DeleteUserCommand(
        Guid UserId
        ) : IRequest<bool>;
}

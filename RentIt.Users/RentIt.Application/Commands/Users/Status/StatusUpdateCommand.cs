using MediatR;

namespace RentIt.Users.Application.Commands.Users.Status
{
    public record StatusUpdateCommand(
        Guid UserId
        ) : IRequest<bool>;
}

using MediatR;
using RentIt.Users.Core.Entities;

namespace RentIt.Users.Application.Queries.Users
{
    public record GetUserByIdQuery(
        Guid UserId
        ) : IRequest<User>;
}

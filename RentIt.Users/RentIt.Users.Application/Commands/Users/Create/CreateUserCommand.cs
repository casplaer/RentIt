using MediatR;
using RentIt.Users.Core.Entities;

namespace RentIt.Users.Application.Commands.Users.Create
{
    public record CreateUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password
        ) : IRequest<Guid>;
} 

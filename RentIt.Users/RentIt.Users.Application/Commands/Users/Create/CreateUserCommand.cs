using MediatR;

namespace RentIt.Users.Application.Commands.Users.Create
{
    public record CreateUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string ConfirmPassword
        ) : IRequest;
} 

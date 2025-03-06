using MediatR;
using RentIt.Users.Contracts.DTO.Users;
using RentIt.Users.Contracts.Responses.Users;

namespace RentIt.Users.Application.Commands.Users.Login
{
    public record LoginUserCommand(string Email, string Password) : IRequest<LoginUserResponse>;
}

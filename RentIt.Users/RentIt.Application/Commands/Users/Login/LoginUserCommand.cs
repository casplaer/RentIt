using MediatR;

namespace RentIt.Users.Application.Commands.Users.Login
{
    public record LoginUserCommand(string Email, string Password) : IRequest<LoginUserResponse>;

    public record LoginUserResponse(string AccessToken);
}

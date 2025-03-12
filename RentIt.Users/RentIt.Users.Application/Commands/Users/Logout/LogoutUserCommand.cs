using MediatR;

namespace RentIt.Users.Application.Commands.Users.Logout
{
    public record LogoutUserCommand(
        string? AccessToken, 
        string? RefreshToken
        ) : IRequest<string>;
}

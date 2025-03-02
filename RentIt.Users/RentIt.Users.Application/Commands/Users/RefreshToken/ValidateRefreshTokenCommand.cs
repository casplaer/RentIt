using MediatR;
using RentIt.Users.Core.Entities;

namespace RentIt.Users.Application.Commands.Users.RefreshToken
{
    public class ValidateRefreshTokenCommand : IRequest<string>
    {
        public string RefreshToken { get; }

        public ValidateRefreshTokenCommand(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
}

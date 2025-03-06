using MediatR;
using RentIt.Users.Contracts.Responses.RefreshToken;

namespace RentIt.Users.Application.Commands.Users.RefreshToken
{
    public class ValidateRefreshTokenCommand : IRequest<ValidateRefreshTokenResponse>
    {
        public string RefreshToken { get; }

        public ValidateRefreshTokenCommand(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
}

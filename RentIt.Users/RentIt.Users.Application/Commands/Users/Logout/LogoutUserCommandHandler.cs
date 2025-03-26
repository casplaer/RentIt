using MediatR;
using Microsoft.AspNetCore.Http;
using RentIt.Users.Application.Interfaces;

namespace RentIt.Users.Application.Commands.Users.Logout
{
    public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, string>
    {
        private readonly IJwtProvider _jwtProvider;

        public LogoutUserCommandHandler(
            IJwtProvider jwtProvider)
        {
            _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
        }

        public async Task<string> Handle(
            LogoutUserCommand request, 
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                await _jwtProvider.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
            }

            if (!string.IsNullOrEmpty(request.AccessToken))
            {
                await _jwtProvider.RevokeAccessTokenAsync(request.AccessToken, cancellationToken);
            }

            return "Пользователь успешно вышел.";
        }
    }
}

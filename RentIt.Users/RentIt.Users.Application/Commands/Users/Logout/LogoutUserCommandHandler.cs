using MediatR;
using Microsoft.AspNetCore.Http;
using RentIt.Users.Application.Interfaces;
using Serilog;

namespace RentIt.Users.Application.Commands.Users.Logout
{
    public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, string>
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly ILogger _logger;

        public LogoutUserCommandHandler(
            IJwtProvider jwtProvider,
            ILogger logger)
        {
            _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            _logger = logger;
        }

        public async Task<string> Handle(
            LogoutUserCommand request, 
            CancellationToken cancellationToken)
        {
            _logger.Information("Попытка выхода пользователя. AccessToken: {AccessToken}, RefreshToken: {RefreshToken}", 
                request.AccessToken, request.RefreshToken);

            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                await _jwtProvider.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

                _logger.Information("Refresh токен отозван успешно.");
            }
            else
            {
                _logger.Warning("Отсутствует refresh токен при выходе пользователя.");
            }

            if (!string.IsNullOrEmpty(request.AccessToken))
            {
                await _jwtProvider.RevokeAccessTokenAsync(request.AccessToken, cancellationToken);

                _logger.Information("Access токен отозван успешно.");
            }
            else
            {
                _logger.Warning("Отсутствует access токен при выходе пользователя.");
            }

            _logger.Information("Пользователь успешно вышел из системы.");

            return "Пользователь успешно вышел.";
        }
    }
}

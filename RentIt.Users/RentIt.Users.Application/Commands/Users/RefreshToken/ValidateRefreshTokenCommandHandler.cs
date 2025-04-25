using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Contracts.Responses.RefreshToken;
using RentIt.Users.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Users.Application.Commands.Users.RefreshToken
{
    public class ValidateRefreshTokenCommandHandler : IRequestHandler<ValidateRefreshTokenCommand, ValidateRefreshTokenResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly ILogger _logger;

        public ValidateRefreshTokenCommandHandler(
            IUserRepository userRepository,
            IJwtProvider jwtProvider,
            ILogger logger)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _logger = logger;
        }

        public async Task<ValidateRefreshTokenResponse> Handle(
            ValidateRefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Проверка refresh токена: {RefreshToken}", request.RefreshToken);

            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                _logger.Warning("Refresh токен пустой.");

                throw new NotFoundException("Требуется повторный вход.");
            }

            var storedRefreshToken = await _jwtProvider.GetStoredTokenAsync(request.RefreshToken, cancellationToken);
            if (storedRefreshToken == null)
            {
                _logger.Warning("Refresh токен не найден или отозван: {RefreshToken}", request.RefreshToken);

                throw new NotFoundException("Требуется повторный вход.");
            }

            var user = await _userRepository.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);
            if (user == null)
            {
                _logger.Warning("Пользователь с данным refresh токеном не найден.");

                throw new NotFoundException("Требуется повторный вход.");
            }

            _logger.Information("Генерация новых токенов для пользователя с ID: {UserId}", user.UserId);

            var newAccessToken = await _jwtProvider.GenerateAccessTokenAsync(user, cancellationToken);
            var newRefreshToken = await _jwtProvider.GenerateRefreshTokenAsync(user, cancellationToken);

            _logger.Information("Новые токены успешно сгенерированы.");

            return new ValidateRefreshTokenResponse(newAccessToken, newRefreshToken);
        }
    }
}

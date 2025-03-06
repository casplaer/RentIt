using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Contracts.Responses.RefreshToken;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.RefreshToken
{
    public class ValidateRefreshTokenCommandHandler : IRequestHandler<ValidateRefreshTokenCommand, ValidateRefreshTokenResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;

        public ValidateRefreshTokenCommandHandler(
            IUserRepository userRepository,
            IJwtProvider jwtProvider)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
        }

        public async Task<ValidateRefreshTokenResponse> Handle(ValidateRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var storedRefreshToken = await _jwtProvider.GetStoredTokenAsync(request.RefreshToken);

            if (string.IsNullOrEmpty(request.RefreshToken) || storedRefreshToken == null)
            {
                throw new NotFoundException("Требуется повторный вход.");
            }

            var user = await _userRepository.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);

            if(user == null)
            {
                throw new NotFoundException("Требуется повторный вход.");
            }

            var newAccessToken = await _jwtProvider.GenerateAccessTokenAsync(user);
            var newRefreshToken = await _jwtProvider.GenerateRefreshTokenAsync(user);

            return new ValidateRefreshTokenResponse(newAccessToken, newRefreshToken);
        }
    }
}

using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Login
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordHasher _passwordHasher;

        public LoginUserCommandHandler(
            IUserRepository userRepository,
            IJwtProvider jwtProvider,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Пользователь не найден."); 
            }

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new Exception("Неверные учетные данные."); 
            }

            var accessToken = _jwtProvider.GenerateAccessToken(user);
            var refreshToken = _jwtProvider.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            return new LoginUserResponse(accessToken);
        }
    }
}

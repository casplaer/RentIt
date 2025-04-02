using AutoMapper;
using MediatR;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Contracts.Dto.Users;
using RentIt.Users.Contracts.Responses.Users;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Login
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailNormalizer _emailNormalizer;
        private readonly IMapper _mapper;

        public LoginUserCommandHandler(
            IUserRepository userRepository,
            IJwtProvider jwtProvider,
            IPasswordHasher passwordHasher,
            IMapper mapper,
            IEmailNormalizer emailNormalizer)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _emailNormalizer = emailNormalizer;
        }

        public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = _emailNormalizer.NormalizeEmail(request.Email);

            var user = await _userRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);

            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Неверные учетные данные.");
            }

            var accessToken = await _jwtProvider.GenerateAccessTokenAsync(user, cancellationToken);
            var refreshToken = await _jwtProvider.GenerateRefreshTokenAsync(user, cancellationToken);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userRepository.SaveChangesAsync(cancellationToken);

            var userDTO = _mapper.Map<UserDto>(user);

            return new LoginUserResponse(accessToken, refreshToken, userDTO);
        }
    }
}

using AutoMapper;
using Moq;
using RentIt.Users.Application.Commands.Users.Login;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Contracts.Dto.Users;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Tests.Commands
{
    public class LoginUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IJwtProvider> _jwtProviderMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IEmailNormalizer> _emailNormalizerMock = new();

        private readonly LoginUserCommandHandler _handler;

        public LoginUserCommandHandlerTests()
        {
            _handler = new LoginUserCommandHandler(
                _userRepositoryMock.Object,
                _jwtProviderMock.Object,
                _passwordHasherMock.Object,
                _mapperMock.Object,
                _emailNormalizerMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnResponse_WhenCredentialsAreValid()
        {
            var user = new User 
            { 
                UserId = Guid.NewGuid(), 
                PasswordHash = "hashedPassword" 
            };

            var command = new LoginUserCommand("test@example.com", "password");

            _emailNormalizerMock.Setup(x => x.NormalizeEmail(command.Email)).Returns("test@example.com");
            _userRepositoryMock.Setup(x => x.GetUserByNormalizedEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.Verify(command.Password, user.PasswordHash)).Returns(true);
            _jwtProviderMock.Setup(x => x.GenerateAccessTokenAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync("access_token");
            _jwtProviderMock.Setup(x => x.GenerateRefreshTokenAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync("refresh_token");
            _mapperMock.Setup(x => x.Map<UserDto>(user)).Returns(new UserDto());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("access_token", result.AccessToken);
            Assert.Equal("refresh_token", result.RefreshToken);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenUserNotFound()
        {
            var command = new LoginUserCommand("notfound@example.com", "password");

            _emailNormalizerMock.Setup(x => x.NormalizeEmail(command.Email)).Returns("notfound@example.com");
            _userRepositoryMock.Setup(x => x.GetUserByNormalizedEmailAsync("notfound@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((User)null);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenPasswordIsInvalid()
        {
            var user = new User
            { 
                UserId = Guid.NewGuid(), 
                PasswordHash = "hashedPassword" 
            };

            var command = new LoginUserCommand("test@example.com", "wrong_password");

            _emailNormalizerMock.Setup(x => x.NormalizeEmail(command.Email)).Returns("test@example.com");
            _userRepositoryMock.Setup(x => x.GetUserByNormalizedEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.Verify(command.Password, user.PasswordHash)).Returns(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldUpdateRefreshToken_AndCallSaveChanges()
        {
            var user = new User
            { 
                UserId = Guid.NewGuid(),
                PasswordHash = "hashedPassword" 
            };

            var command = new LoginUserCommand("test@example.com", "password");

            _emailNormalizerMock.Setup(x => x.NormalizeEmail(command.Email)).Returns("test@example.com");
            _userRepositoryMock.Setup(x => x.GetUserByNormalizedEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.Verify(command.Password, user.PasswordHash)).Returns(true);
            _jwtProviderMock.Setup(x => x.GenerateAccessTokenAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync("access_token");
            _jwtProviderMock.Setup(x => x.GenerateRefreshTokenAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync("refresh_token");
            _mapperMock.Setup(x => x.Map<UserDto>(user)).Returns(new UserDto());

            await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("refresh_token", user.RefreshToken);

            _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

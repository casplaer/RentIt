using Moq;
using RentIt.Users.Application.Commands.Users.RefreshToken;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Tests.Commands
{
    public class ValidateRefreshTokenCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IJwtProvider> _jwtProviderMock;
        private readonly ValidateRefreshTokenCommandHandler _handler;

        public ValidateRefreshTokenCommandHandlerTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _jwtProviderMock = new Mock<IJwtProvider>();
            _handler = new ValidateRefreshTokenCommandHandler(_userRepoMock.Object, _jwtProviderMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            var user = new User 
            { 
                UserId = Guid.NewGuid() 
            };

            var command = new ValidateRefreshTokenCommand("valid-refresh-token");
            var storedToken = "valid-refresh-token";
            var newAccessToken = "new-access-token";
            var newRefreshToken = "new-refresh-token";

            _jwtProviderMock.Setup(x => x.GetStoredTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(storedToken);
            _userRepoMock.Setup(x => x.GetUserByRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);
            _jwtProviderMock.Setup(x => x.GenerateAccessTokenAsync(user, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(newAccessToken);
            _jwtProviderMock.Setup(x => x.GenerateRefreshTokenAsync(user, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(newRefreshToken);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(newAccessToken, result.NewAccessToken);
            Assert.Equal(newRefreshToken, result.NewRefreshToken);

            _jwtProviderMock.Verify(x => x.GenerateAccessTokenAsync(user, It.IsAny<CancellationToken>()), Times.Once);
            _jwtProviderMock.Verify(x => x.GenerateRefreshTokenAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenRefreshTokenIsNullOrEmpty()
        {
            var command = new ValidateRefreshTokenCommand("");

            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Требуется повторный вход.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenStoredTokenIsNull()
        {
            var command = new ValidateRefreshTokenCommand("invalid-refresh-token");

            _jwtProviderMock.Setup(x => x.GetStoredTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((string)null); 

            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Требуется повторный вход.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenUserIsNotFoundByRefreshToken()
        {
            var command = new ValidateRefreshTokenCommand("valid-refresh-token");
            var storedToken = "valid-refresh-token";

            _jwtProviderMock.Setup(x => x.GetStoredTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(storedToken);
            _userRepoMock.Setup(x => x.GetUserByRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
                         .ReturnsAsync((User)null); 

            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Требуется повторный вход.", exception.Message);
        }
    }
}

using Moq;
using RentIt.Users.Application.Commands.Users.Logout;
using RentIt.Users.Application.Interfaces;

namespace RentIt.Users.Tests.Commands
{
    public class LogoutUserCommandHandlerTests
    {
        private readonly Mock<IJwtProvider> _jwtProviderMock = new();
        private readonly LogoutUserCommandHandler _handler;

        public LogoutUserCommandHandlerTests()
        {
            _handler = new LogoutUserCommandHandler(_jwtProviderMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldRevokeTokens_WhenTokensAreProvided()
        {
            var command = new LogoutUserCommand("access_token_example", "refresh_token_example");

            _jwtProviderMock.Setup(x => x.RevokeAccessTokenAsync(command.AccessToken!, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            _jwtProviderMock.Setup(x => x.RevokeRefreshTokenAsync(command.RefreshToken!, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("Пользователь успешно вышел.", result);

            _jwtProviderMock.Verify(x => x.RevokeAccessTokenAsync(command.AccessToken!, It.IsAny<CancellationToken>()), Times.Once);
            _jwtProviderMock.Verify(x => x.RevokeRefreshTokenAsync(command.RefreshToken!, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldSkipRevocation_WhenTokensAreNullOrEmpty()
        {
            var command = new LogoutUserCommand(null, "");

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("Пользователь успешно вышел.", result);

            _jwtProviderMock.Verify(x => x.RevokeAccessTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _jwtProviderMock.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

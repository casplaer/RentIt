using Moq;
using RentIt.Users.Application.Commands.Users.Account;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Tests.Commands
{
    public class ConfirmAccountCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IAccountTokenRepository> _mockAccountTokenRepository;
        private readonly ConfirmAccountCommandHandler _handler;

        public ConfirmAccountCommandHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockAccountTokenRepository = new Mock<IAccountTokenRepository>();
            _handler = new ConfirmAccountCommandHandler(
                _mockUserRepository.Object,
                _mockAccountTokenRepository.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldActivateUser_WhenTokenIsValid()
        {
            var userId = Guid.NewGuid();
            var token = "validToken";

            var user = new User 
            { 
                UserId = userId, 
                Status = UserStatus.Unconfirmed 
            };

            var tokenEntity = new AccountToken
            {
                UserId = userId,
                Token = token,
                TokenType = TokenType.Confirmation,
                Expiration = DateTime.UtcNow.AddHours(1)
            };

            _mockAccountTokenRepository.Setup(x => x.GetTokenAsync(userId, token, TokenType.Confirmation, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenEntity);

            _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var result = await _handler.Handle(new ConfirmAccountCommand(userId, token), CancellationToken.None);

            Assert.True(result);
            Assert.Equal(UserStatus.Active, user.Status);

            _mockAccountTokenRepository.Verify(x => x.Delete(tokenEntity), Times.Once);
            _mockUserRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenTokenIsInvalidOrExpired()
        {
            var userId = Guid.NewGuid();
            var token = "invalidToken";

            _mockAccountTokenRepository.Setup(x => x.GetTokenAsync(userId, token, TokenType.Confirmation, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AccountToken)null);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
                    _handler.Handle(new ConfirmAccountCommand(userId, token), CancellationToken.None));
            
            Assert.Equal("Неверная или просроченная ссылка для восстановления пароля.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            var token = "validToken";

            var tokenEntity = new AccountToken
            {
                UserId = userId,
                Token = token,
                TokenType = TokenType.Confirmation,
                Expiration = DateTime.UtcNow.AddHours(1)
            };

            _mockAccountTokenRepository.Setup(x => x.GetTokenAsync(userId, token, TokenType.Confirmation, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenEntity);

            _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
                    _handler.Handle(new ConfirmAccountCommand(userId, token), CancellationToken.None));
            
            Assert.Equal("Пользователь не найден.", exception.Message);
        }
    }
}

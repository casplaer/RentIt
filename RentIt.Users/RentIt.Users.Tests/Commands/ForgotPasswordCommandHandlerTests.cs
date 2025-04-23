using Moq;
using RentIt.Users.Application.Commands.Users.Password;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Tests.Commands
{
    public class ForgotPasswordCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IAccountTokenRepository> _tokenRepoMock = new();
        private readonly Mock<IEmailSender> _emailSenderMock = new();
        private readonly Mock<IEmailNormalizer> _emailNormalizerMock = new();
        private readonly Mock<IAccountTokenGenerator> _tokenGenMock = new();
        private readonly Mock<ILinkGenerator> _linkGenMock = new();
        private readonly ForgotPasswordCommandHandler _handler;

        public ForgotPasswordCommandHandlerTests()
        {
            _handler = new ForgotPasswordCommandHandler(
                _userRepoMock.Object,
                _tokenRepoMock.Object,
                _emailSenderMock.Object,
                _emailNormalizerMock.Object,
                _tokenGenMock.Object,
                _linkGenMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldSendResetEmail_WhenUserExists()
        {
            var email = "test@example.com";
            var normalizedEmail = "test@example.com";
            var token = "generated_token";
            var command = new ForgotPasswordCommand(email);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = email
            };

            _emailNormalizerMock.Setup(x => x.NormalizeEmail(email)).Returns(normalizedEmail);
            _userRepoMock.Setup(x => x.GetUserByNormalizedEmailAsync(normalizedEmail, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);
            _tokenGenMock.Setup(x => x.GenerateToken(It.IsAny<int>())).Returns(token);
            _linkGenMock.Setup(x => x.GenerateResetPasswordLink(email, token)).Returns("https://link");

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            
            _tokenRepoMock.Verify(x => x.AddAsync(It.Is<AccountToken>(t =>
                t.UserId == user.UserId &&
                t.Token == token &&
                t.TokenType == TokenType.PasswordReset), It.IsAny<CancellationToken>()), Times.Once);

            _emailSenderMock.Verify(x => x.SendEmailAsync(
                email,
                It.Is<string>(s => s.Contains("Восстановление пароля")),
                It.Is<string>(b => b.Contains("https://link")),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenUserNotExists()
        {
            var email = "notfound@example.com";

            _emailNormalizerMock.Setup(x => x.NormalizeEmail(email)).Returns(email);
            _userRepoMock.Setup(x => x.GetUserByNormalizedEmailAsync(email, It.IsAny<CancellationToken>()))
                         .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(new ForgotPasswordCommand(email), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldSaveTokenAndEmailOnlyOnce()
        {
            var email = "test@example.com";
            var normalizedEmail = "test@example.com";
            var token = "123456token";

            var user = new User 
            { 
                UserId = Guid.NewGuid(), 
                Email = email 
            };

            _emailNormalizerMock.Setup(x => x.NormalizeEmail(email)).Returns(normalizedEmail);
            _userRepoMock.Setup(x => x.GetUserByNormalizedEmailAsync(normalizedEmail, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);
            _tokenGenMock.Setup(x => x.GenerateToken(It.IsAny<int>())).Returns(token);
            _linkGenMock.Setup(x => x.GenerateResetPasswordLink(email, token)).Returns("https://reset.link");

            await _handler.Handle(new ForgotPasswordCommand(email), CancellationToken.None);

            _tokenRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _emailSenderMock.Verify(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

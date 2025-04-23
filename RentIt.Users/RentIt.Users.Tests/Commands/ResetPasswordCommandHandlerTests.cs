using FluentValidation.Results;
using FluentValidation;
using Moq;
using RentIt.Users.Application.Commands.Users.Password;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;
using RentIt.Users.Application.Validators;

namespace RentIt.Users.Tests.Commands
{
    public class ResetPasswordCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IAccountTokenRepository> _tokenRepoMock = new();
        private readonly Mock<IEmailNormalizer> _emailNormalizerMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<IValidator<ResetPasswordCommand>> _validatorMock = new();

        private readonly ResetPasswordCommandHandler _handler;

        public ResetPasswordCommandHandlerTests()
        {
            _handler = new ResetPasswordCommandHandler(
                _userRepoMock.Object,
                _tokenRepoMock.Object,
                _passwordHasherMock.Object,
                _emailNormalizerMock.Object,
                new ResetPasswordCommandValidator());
        }

        [Fact]
        public async Task Handle_ShouldResetPassword_WhenDataIsValid()
        {
            var email = "test@example.com";
            var normalizedEmail = "test@example.com";
            var user = new User { UserId = Guid.NewGuid(), Email = email };

            var token = new AccountToken
            {
                Token = "valid-token",
                UserId = user.UserId,
                TokenType = TokenType.PasswordReset,
                Expiration = DateTime.UtcNow.AddMinutes(30)
            };

            var command = new ResetPasswordCommand
            {
                Email = email,
                Token = "valid-token",
                NewPassword = "NewSecure123!",
                ConfirmPassword = "NewSecure123!"
            };

            _emailNormalizerMock.Setup(x => x.NormalizeEmail(email)).Returns(normalizedEmail);
            _userRepoMock.Setup(x => x.GetUserByNormalizedEmailAsync(normalizedEmail, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _tokenRepoMock.Setup(x => x.GetTokenAsync(command.Token, TokenType.PasswordReset, It.IsAny<CancellationToken>())).ReturnsAsync(token);
            _validatorMock.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());
            _passwordHasherMock.Setup(x => x.Hash(command.NewPassword)).Returns("hashed_password");

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            Assert.Equal("hashed_password", user.PasswordHash);

            _userRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenUserNotFound()
        {
            _emailNormalizerMock.Setup(x => x.NormalizeEmail(It.IsAny<string>())).Returns("normalized");
            _userRepoMock.Setup(x => x.GetUserByNormalizedEmailAsync("normalized", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(new ResetPasswordCommand { Email = "nonexistent@mail.com" }, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowArgumentException_WhenTokenIsInvalid()
        {
            var user = new User { UserId = Guid.NewGuid() };
            var token = new AccountToken
            {
                Token = "expired",
                UserId = user.UserId,
                TokenType = TokenType.PasswordReset,
                Expiration = DateTime.UtcNow.AddHours(-1)
            };

            _emailNormalizerMock.Setup(x => x.NormalizeEmail(It.IsAny<string>())).Returns("email");
            _userRepoMock.Setup(x => x.GetUserByNormalizedEmailAsync("email", It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _tokenRepoMock.Setup(x => x.GetTokenAsync("expired", TokenType.PasswordReset, It.IsAny<CancellationToken>())).ReturnsAsync(token);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.Handle(new ResetPasswordCommand { Email = "email", Token = "expired" }, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
        {
            var user = new User { UserId = Guid.NewGuid() };
            
            var token = new AccountToken
            {
                Token = "token",
                UserId = user.UserId,
                TokenType = TokenType.PasswordReset,
                Expiration = DateTime.UtcNow.AddHours(1)
            };

            var command = new ResetPasswordCommand
            {
                Email = "user@mail.com",
                Token = "token",
                NewPassword = "short",
                ConfirmPassword = "short"
            };

            _emailNormalizerMock.Setup(x => x.NormalizeEmail(command.Email)).Returns(command.Email);
            _userRepoMock.Setup(x => x.GetUserByNormalizedEmailAsync(command.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _tokenRepoMock.Setup(x => x.GetTokenAsync("token", TokenType.PasswordReset, It.IsAny<CancellationToken>())).ReturnsAsync(token);

            _validatorMock.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure("NewPassword", "Too short.")
                }));

            await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }
    }
}
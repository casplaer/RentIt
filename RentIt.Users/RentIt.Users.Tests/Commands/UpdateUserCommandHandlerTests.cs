using AutoMapper;
using Moq;
using RentIt.Users.Application.Commands.Users.Update;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Tests.Commands
{
    public class UpdateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IEmailNormalizer> _emailNormalizerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateUserCommandHandler _handler;

        public UpdateUserCommandHandlerTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _emailNormalizerMock = new Mock<IEmailNormalizer>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdateUserCommandHandler(_userRepoMock.Object, _emailNormalizerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldUpdateUser_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var existingUser = new User 
            { 
                UserId = userId, 
                FirstName = "OldFirstName", 
                LastName = "OldLastName" 
            };

            var command = new UpdateUserCommand(userId, 
                                                "NewFirstName", 
                                                "NewLastName", 
                                                "newemail@example.com", 
                                                "123456789", 
                                                "Country", 
                                                "City", 
                                                "Address");

            _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(existingUser);

            _emailNormalizerMock.Setup(x => x.NormalizeEmail(It.IsAny<string>())).Returns("newemail@example.com");

            _mapperMock.Setup(x => x.Map(It.IsAny<object>(), It.IsAny<object>())).Callback<object, object>((src, dest) =>
            {
                var srcCommand = src as UpdateUserCommand;
                var destUser = dest as User;

                if (srcCommand != null && destUser != null)
                {
                    destUser.FirstName = srcCommand.FirstName;
                    destUser.LastName = srcCommand.LastName;
                }
            });

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            Assert.Equal("NewFirstName", existingUser.FirstName);
            Assert.Equal("NewLastName", existingUser.LastName);
            Assert.Equal("newemail@example.com", existingUser.NormalizedEmail);

            _userRepoMock.Verify(x => x.Update(existingUser), Times.Once);
            _userRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            var command = new UpdateUserCommand(userId, 
                                                "FirstName", 
                                                "LastName", 
                                                "email@example.com", 
                                                "987654321", 
                                                "Country", 
                                                "City", 
                                                "Address");

            _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync((User)null);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Пользователь не найден.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldNotUpdate_WhenEmailNormalizationFails()
        {
            var userId = Guid.NewGuid();
            var existingUser = new User 
            { 
                UserId = userId, 
                FirstName = "OldFirstName", 
                LastName = "OldLastName" 
            };

            var command = new UpdateUserCommand(userId, 
                                                "NewFirstName", 
                                                "NewLastName", 
                                                "invalid-email", 
                                                "123456789", 
                                                "Country", 
                                                "City", 
                                                "Address");

            _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(existingUser);

            _emailNormalizerMock.Setup(x => x.NormalizeEmail(It.IsAny<string>())).Throws(new FormatException("Invalid email"));

            var exception = await Assert.ThrowsAsync<FormatException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Invalid email", exception.Message);
        }
    }
}

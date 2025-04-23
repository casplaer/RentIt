using Moq;
using RentIt.Users.Application.Commands.Users.Status;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Tests.Commands
{
    public class StatusUpdateCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly StatusUpdateCommandHandler _handler;

        public StatusUpdateCommandHandlerTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _handler = new StatusUpdateCommandHandler(_userRepoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldActivateUser_WhenUserIsInactive()
        {
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId, Status = UserStatus.Inactive };
            var command = new StatusUpdateCommand(userId);

            _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            Assert.Equal(UserStatus.Active, user.Status);

            _userRepoMock.Verify(x => x.Update(user), Times.Once);
            _userRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldDeactivateUser_WhenUserIsActive()
        {
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId, Status = UserStatus.Active };
            var command = new StatusUpdateCommand(userId);

            _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            Assert.Equal(UserStatus.Inactive, user.Status);

            _userRepoMock.Verify(x => x.Update(user), Times.Once);
            _userRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            var command = new StatusUpdateCommand(userId);

            _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync((User)null);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Пользователь не найден.", exception.Message);
        }
    }
}

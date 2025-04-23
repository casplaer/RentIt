using Moq;
using RentIt.Users.Application.Commands.Users.Role;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Tests.Commands
{
    public class UpdateUserRoleCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IRoleRepository> _roleRepoMock;
        private readonly UpdateUserRoleCommandHandler _handler;

        public UpdateUserRoleCommandHandlerTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _roleRepoMock = new Mock<IRoleRepository>();
            _handler = new UpdateUserRoleCommandHandler(_userRepoMock.Object, _roleRepoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldUpdateUserRole_WhenUserAndRoleExist()
        {
            var userId = Guid.NewGuid();
            var user = new User
            { 
                UserId = userId, 
                RoleId = Guid.NewGuid() 
            };

            var newRole = new Role
            { 
                RoleId = Guid.NewGuid(), 
                RoleName = "Admin" 
            };

            var command = new UpdateUserRoleCommand(userId, "Admin");

            _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);

            _roleRepoMock.Setup(x => x.GetRoleByNameAsync("Admin", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(newRole);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            Assert.Equal(newRole.RoleId, user.RoleId);
            Assert.Equal(newRole, user.Role);

            _userRepoMock.Verify(x => x.Update(user), Times.Once);
            _userRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            var command = new UpdateUserRoleCommand(userId, "Admin");

            _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync((User)null);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Пользователь не найден.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenRoleNotFound()
        {
            var userId = Guid.NewGuid();
            var user = new User 
            { 
                UserId = userId 
            };

            var command = new UpdateUserRoleCommand(userId, "Admin");

            _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);

            _roleRepoMock.Setup(x => x.GetRoleByNameAsync("Admin", It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Role)null);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Роль Admin не найдена.", exception.Message);
        }
    }
}

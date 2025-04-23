using Moq;
using RentIt.Users.Application.Commands.Users.Delete;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Tests.Commands
{
    public class DeleteUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly DeleteUserCommandHandler _handler;

        public DeleteUserCommandHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _handler = new DeleteUserCommandHandler(_mockUserRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            var command = new DeleteUserCommand(Guid.NewGuid());

            _mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((User)null);  

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result); 
            
            _mockUserRepository.Verify(x => x.Delete(It.IsAny<User>()), Times.Never); 
            _mockUserRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId };
            var command = new DeleteUserCommand(userId);

            _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(user); 

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);  
            
            _mockUserRepository.Verify(x => x.Delete(user), Times.Once); 
            _mockUserRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldDeleteUser_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId };
            var command = new DeleteUserCommand(userId);

            _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(user);

            var result = await _handler.Handle(command, CancellationToken.None);

            _mockUserRepository.Verify(x => x.Delete(It.Is<User>(u => u.UserId == userId)), Times.Once); 
        }
    }
}

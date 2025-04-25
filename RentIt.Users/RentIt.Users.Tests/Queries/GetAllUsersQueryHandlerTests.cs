using Moq;
using RentIt.Users.Application.Queries.Users;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Tests.Queries
{
    public class GetAllUsersQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly GetAllUsersQueryHandler _handler;

        public GetAllUsersQueryHandlerTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _handler = new GetAllUsersQueryHandler(_userRepoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnUsers_WhenUsersExist()
        {
            var users = new List<User>
        {
            new User 
            { 
                UserId = Guid.NewGuid(), 
                FirstName = "John", 
                LastName = "Doe" 
            },
            new User 
            { 
                UserId = Guid.NewGuid(), 
                FirstName = "Jane", 
                LastName = "Smith" 
            }
        };

            _userRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(users);

            var query = new GetAllUsersQuery();

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("John", result.First().FirstName);
            Assert.Equal("Jane", result.Last().FirstName);

            _userRepoMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            _userRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<User>());

            var query = new GetAllUsersQuery();

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);

            _userRepoMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            _userRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new Exception("Database error"));

            var query = new GetAllUsersQuery();

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        }
    }
}
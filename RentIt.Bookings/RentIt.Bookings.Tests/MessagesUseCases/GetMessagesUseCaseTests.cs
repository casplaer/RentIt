using Moq;
using RentIt.Bookings.Application.UseCases.Messages;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Tests.MessagesUseCases
{
    public class GetMessagesUseCaseTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly GetMessagesUseCase _useCase;

        public GetMessagesUseCaseTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger>();
            _useCase = new GetMessagesUseCase(
                _unitOfWorkMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenValidUserId_ReturnsMessages()
        {
            var userId = Guid.NewGuid().ToString();
            var otherUserId = Guid.NewGuid();
            var messages = new List<Message>
            {
                new Message { Id = Guid.NewGuid(), SenderId = Guid.NewGuid(), ReceiverId = otherUserId, Content = "Message 1" },
                new Message { Id = Guid.NewGuid(), SenderId = otherUserId, ReceiverId = Guid.NewGuid(), Content = "Message 2" }
            };

            _unitOfWorkMock.Setup(x => x.Messages.GetMessagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(messages);

            var result = await _useCase.ExecuteAsync(userId, otherUserId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _unitOfWorkMock.Verify(u => u.Messages.GetMessagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenInvalidUserIdFormat_ThrowsArgumentException()
        {
            var invalidUserId = "invalid-id";
            var otherUserId = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(invalidUserId, otherUserId, CancellationToken.None));

            Assert.Equal("Некорректный формат ID пользователя.", exception.Message);
            _loggerMock.Verify(l => l.Warning("Некорректный формат ID пользователя, ожидался GUID."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenMessagesRepositoryFails_ThrowsException()
        {
            var userId = Guid.NewGuid().ToString();
            var otherUserId = Guid.NewGuid();
            var userGuid = Guid.Parse(userId);

            _unitOfWorkMock.Setup(x => x.Messages.GetMessagesAsync(userGuid, otherUserId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Ошибка запроса сообщений"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync(userId, otherUserId, CancellationToken.None));

            Assert.Equal("Ошибка запроса сообщений", exception.Message);
        }
    }
}
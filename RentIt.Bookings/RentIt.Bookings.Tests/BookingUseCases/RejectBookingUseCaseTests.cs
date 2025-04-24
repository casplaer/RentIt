using Moq;
using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.Services.Grpc;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.UseCases.Bookings;
using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Tests.BookingUseCases
{
    public class RejectBookingUseCaseTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IHousingIntegrationService> _housingIntegrationServiceMock = new();
        private readonly Mock<IBookingNotificationService> _bookingNotificationServiceMock = new();
        private readonly RejectBookingUseCase _useCase;

        public RejectBookingUseCaseTests()
        {
            _useCase = new RejectBookingUseCase(
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _housingIntegrationServiceMock.Object,
                _bookingNotificationServiceMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingExists_ChangesStatusToRejected()
        {
            var userId = Guid.NewGuid().ToString();
            var bookingId = Guid.NewGuid();
            var userGuid = Guid.NewGuid();  

            var booking = new Booking
            {
                BookingId = bookingId,
                Status = BookingStatus.Pending,
                HousingId = Guid.NewGuid(),
                UserId = userGuid
            };

            var housing = new HousingInfoDto(
                OwnerId: userGuid,
                HousingName: "Test Housing",
                PricePerNight: 100);

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            _housingIntegrationServiceMock.Setup(x => x.GetHousingInfoAsync(booking.HousingId))
                .ReturnsAsync(housing);

            await _useCase.ExecuteAsync(userGuid.ToString(), bookingId, CancellationToken.None);

            Assert.Equal(BookingStatus.Rejected, booking.Status);
            _unitOfWorkMock.Verify(u => u.Bookings.Update(booking), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _bookingNotificationServiceMock.Verify(s => s.NotifyUserAboutBookingRejectionAsync(booking, housing, It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task ExecuteAsync_WhenBookingNotFound_ThrowsNotFoundException()
        {
            var userId = Guid.NewGuid().ToString();
            var bookingId = Guid.NewGuid();

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking)null);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, bookingId, CancellationToken.None));

            Assert.Equal("Бронирование с таким ID не найдено.", exception.Message);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserNotOwner_ThrowsUnauthorizedAccessException()
        {
            var userId = Guid.NewGuid().ToString();
            var bookingId = Guid.NewGuid();
            var booking = new Booking
            {
                BookingId = bookingId,
                Status = BookingStatus.Pending,
                HousingId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            var housing = new HousingInfoDto(
                OwnerId: Guid.NewGuid(), 
                HousingName: "Test Housing",
                PricePerNight: 100);

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            _housingIntegrationServiceMock.Setup(x => x.GetHousingInfoAsync(booking.HousingId))
                .ReturnsAsync(housing);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, bookingId, CancellationToken.None));

            Assert.Equal("Попытка неавторизованного доступа.", exception.Message);
        }

        [Fact]
        public async Task ExecuteAsync_WhenInvalidUserId_ThrowsArgumentException()
        {
            var userId = "invalid-user-id";
            var bookingId = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(userId, bookingId, CancellationToken.None));

            Assert.Equal("Некорректный формат ID пользователя.", exception.Message);
        }

        [Fact]
        public async Task ExecuteAsync_WhenExceptionThrown_LogsError()
        {
            var userId = Guid.NewGuid().ToString();
            var bookingId = Guid.NewGuid();

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync(userId, bookingId, CancellationToken.None));
        }
    }
}

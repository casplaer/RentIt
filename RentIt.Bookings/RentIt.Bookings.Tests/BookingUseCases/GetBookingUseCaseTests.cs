using Moq;
using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.Services.Grpc;
using RentIt.Bookings.Application.UseCases.Bookings;
using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Tests.BookingUseCases
{
    public class GetBookingUseCaseTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IHousingIntegrationService> _housingServiceMock = new();
        private readonly GetBookingUseCase _useCase;

        public GetBookingUseCaseTests()
        {
            _useCase = new GetBookingUseCase(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _housingServiceMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIsAuthorized_ReturnsBooking()
        {
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var housingId = Guid.NewGuid();

            var booking = new Booking { BookingId = bookingId, UserId = userId, HousingId = housingId };
            var housing = new HousingInfoDto(userId, "Test Housing", 100);

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            _housingServiceMock.Setup(x => x.GetHousingInfoAsync(housingId))
                .ReturnsAsync(housing);

            var result = await _useCase.ExecuteAsync(bookingId, userId.ToString(), "User", CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(bookingId, result.BookingId);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIsOwner_ReturnsBooking()
        {
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var housingId = Guid.NewGuid();

            var booking = new Booking { BookingId = bookingId, UserId = userId, HousingId = housingId };
            var housing = new HousingInfoDto(ownerId, "Test", 100);

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            _housingServiceMock.Setup(x => x.GetHousingInfoAsync(housingId))
                .ReturnsAsync(housing);

            var result = await _useCase.ExecuteAsync(bookingId, ownerId.ToString(), "User", CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(bookingId, result.BookingId);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIsAdmin_ReturnsBooking()
        {
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var housingId = Guid.NewGuid();

            var booking = new Booking { BookingId = bookingId, UserId = userId, HousingId = housingId };
            var housing = new HousingInfoDto(Guid.NewGuid(), "Test", 100);

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            _housingServiceMock.Setup(x => x.GetHousingInfoAsync(housingId))
                .ReturnsAsync(housing);

            var result = await _useCase.ExecuteAsync(bookingId, Guid.NewGuid().ToString(), "Admin", CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(bookingId, result.BookingId);
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingNotFound_ThrowsNotFoundException()
        {
            var bookingId = Guid.NewGuid();

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _useCase.ExecuteAsync(bookingId, Guid.NewGuid().ToString(), "User", CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIsNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var housingId = Guid.NewGuid();

            var booking = new Booking { BookingId = bookingId, UserId = userId, HousingId = housingId };
            var housing = new HousingInfoDto(Guid.NewGuid(), "Test", 100);

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            _housingServiceMock.Setup(x => x.GetHousingInfoAsync(housingId))
                .ReturnsAsync(housing);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _useCase.ExecuteAsync(bookingId, Guid.NewGuid().ToString(), "User", CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIdIsInvalid_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _useCase.ExecuteAsync(Guid.NewGuid(), "invalid-guid", "User", CancellationToken.None));
        }
    }
}
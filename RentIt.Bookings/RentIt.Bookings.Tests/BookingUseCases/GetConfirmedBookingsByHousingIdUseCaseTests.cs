using Moq;
using RentIt.Bookings.Application.Specifications.Bookings;
using RentIt.Bookings.Application.UseCases.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Tests.BookingUseCases
{
    public class GetConfirmedBookingsByHousingIdUseCaseTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly GetConfirmedBookingsByHousingIdUseCase _useCase;

        public GetConfirmedBookingsByHousingIdUseCaseTests()
        {
            _useCase = new GetConfirmedBookingsByHousingIdUseCase(
                _loggerMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenConfirmedBookingsExist_ReturnsConfirmedBookings()
        {
            var housingId = Guid.NewGuid();
            var confirmedBooking1 = new Booking { BookingId = Guid.NewGuid(), HousingId = housingId, Status = BookingStatus.Confirmed };
            var confirmedBooking2 = new Booking { BookingId = Guid.NewGuid(), HousingId = housingId, Status = BookingStatus.Confirmed };

            var confirmedBookings = new List<Booking> { confirmedBooking1, confirmedBooking2 };

            _unitOfWorkMock.Setup(x => x.Bookings.GetAllFilteredBookingsAsync(It.IsAny<SearchBookingSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(confirmedBookings);

            var result = await _useCase.ExecuteAsync(housingId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, booking => Assert.Equal(BookingStatus.Confirmed, booking.Status));
        }

        [Fact]
        public async Task ExecuteAsync_WhenNoConfirmedBookingsExist_ReturnsEmptyList()
        {
            var housingId = Guid.NewGuid();

            _unitOfWorkMock.Setup(x => x.Bookings.GetAllFilteredBookingsAsync(It.IsAny<SearchBookingSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Booking>());

            var result = await _useCase.ExecuteAsync(housingId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExecuteAsync_WhenInvalidHousingId_ThrowsNotFoundException()
        {
            var invalidHousingId = Guid.NewGuid();

            _unitOfWorkMock.Setup(x => x.Bookings.GetAllFilteredBookingsAsync(It.IsAny<SearchBookingSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Booking>());

            var result = await _useCase.ExecuteAsync(invalidHousingId, CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ExecuteAsync_WhenExceptionThrown_LogsError()
        {
            var housingId = Guid.NewGuid();

            _unitOfWorkMock.Setup(x => x.Bookings.GetAllFilteredBookingsAsync(It.IsAny<SearchBookingSpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync(housingId, CancellationToken.None));
        }
    }
}
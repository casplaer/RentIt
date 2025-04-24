using Moq;
using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.UseCases.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Tests.BookingUseCases
{
    public class DeleteBookingUseCaseTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly DeleteBookingUseCase _useCase;

        public DeleteBookingUseCaseTests()
        {
            _useCase = new DeleteBookingUseCase(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingExists_DeletesBookingSuccessfully()
        {
            var bookingId = Guid.NewGuid();
            var booking = new Booking { BookingId = bookingId };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            await _useCase.ExecuteAsync(bookingId, CancellationToken.None);

            _unitOfWorkMock.Verify(x => x.Bookings.Delete(booking), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingDoesNotExist_ThrowsNotFoundException()
        {
            var bookingId = Guid.NewGuid();

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _useCase.ExecuteAsync(bookingId, CancellationToken.None));
        }
    }
}
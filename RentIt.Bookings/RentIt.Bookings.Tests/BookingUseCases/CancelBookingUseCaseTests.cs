using Moq;
using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.EventBus;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using RentIt.Bookings.Application.UseCases.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.MessageBroker.Contracts.Events;
using Serilog;

namespace RentIt.Bookings.Tests.BookingUseCases
{
    public class CancelBookingUseCaseTests
    {
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IEventBus> _eventBusMock = new();
        private readonly Mock<IRefundPaymentUseCase> _refundPaymentUseCaseMock = new();
        private readonly Mock<IBookingNotificationService> _notificationServiceMock = new();

        private readonly CancelBookingUseCase _useCase;

        public CancelBookingUseCaseTests()
        {
            _useCase = new CancelBookingUseCase(
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _eventBusMock.Object,
                _refundPaymentUseCaseMock.Object,
                _notificationServiceMock.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_WhenValidRequest_CancelsBookingSuccessfully()
        {
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var booking = new Booking
            {
                BookingId = bookingId,
                HousingId = Guid.NewGuid(),
                UserId = userId,
                Status = BookingStatus.Confirmed,
                StartDate = DateTime.UtcNow.AddDays(2),
                Payment = null
            };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            _unitOfWorkMock.Setup(x => x.Bookings.GetCurrentBookingChainAsync(booking, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DateTime.Today, DateTime.Today.AddDays(3)));

            await _useCase.ExecuteAsync(bookingId, userId.ToString(), CancellationToken.None);

            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            _unitOfWorkMock.Verify(x => x.Bookings.Update(booking), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<BookingUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _notificationServiceMock.Verify(x => x.NotifyUserAboutBookingCancellationAsync(booking, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingPaid_RefundsBeforeCancel()
        {
            var userId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                HousingId = Guid.NewGuid(),
                UserId = userId,
                Status = BookingStatus.Paid,
                StartDate = DateTime.UtcNow.AddDays(3),
                Payment = new Payment { PaymentId = paymentId }
            };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(booking.BookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            _unitOfWorkMock.Setup(x => x.Bookings.GetCurrentBookingChainAsync(booking, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DateTime.Today, DateTime.Today.AddDays(5)));

            await _useCase.ExecuteAsync(booking.BookingId, userId.ToString(), CancellationToken.None);

            _refundPaymentUseCaseMock.Verify(x => x.ExecuteAsync(paymentId, false, It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(BookingStatus.Cancelled, booking.Status);
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingNotFound_ThrowsNotFoundException()
        {
            var bookingId = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _useCase.ExecuteAsync(bookingId, Guid.NewGuid().ToString(), CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIdIsInvalid_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _useCase.ExecuteAsync(Guid.NewGuid(), "invalid-guid", CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenUnauthorizedAccess_ThrowsUnauthorizedAccessException()
        {
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(booking.BookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _useCase.ExecuteAsync(booking.BookingId, Guid.NewGuid().ToString(), CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenStatusIsInvalid_ThrowsArgumentException()
        {
            var userId = Guid.NewGuid();
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                UserId = userId,
                Status = BookingStatus.Cancelled,
                StartDate = DateTime.UtcNow.AddDays(2)
            };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(booking.BookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _useCase.ExecuteAsync(booking.BookingId, userId.ToString(), CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenLessThan24Hours_ThrowsArgumentException()
        {
            var userId = Guid.NewGuid();
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                UserId = userId,
                Status = BookingStatus.Confirmed,
                StartDate = DateTime.UtcNow.AddHours(12)
            };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(booking.BookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _useCase.ExecuteAsync(booking.BookingId, userId.ToString(), CancellationToken.None));
        }
    }
}

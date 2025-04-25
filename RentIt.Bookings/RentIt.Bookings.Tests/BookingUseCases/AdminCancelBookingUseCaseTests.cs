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
    public class AdminCancelBookingUseCaseTests
    {
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IEventBus> _eventBusMock = new();
        private readonly Mock<IRefundPaymentUseCase> _refundPaymentUseCaseMock = new();
        private readonly Mock<IBookingNotificationService> _notificationServiceMock = new();

        private readonly AdminCancelBookingUseCase _useCase;

        public AdminCancelBookingUseCaseTests()
        {
            _useCase = new AdminCancelBookingUseCase(
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _eventBusMock.Object,
                _refundPaymentUseCaseMock.Object,
                _notificationServiceMock.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingExistsWithoutPayment_CancelsBookingAndPublishesEvent()
        {
            var bookingId = Guid.NewGuid();
            var booking = new Booking
            {
                BookingId = bookingId,
                HousingId = Guid.NewGuid(),
                Status = BookingStatus.Confirmed,
                Payment = null
            };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            _unitOfWorkMock.Setup(x => x.Bookings.GetCurrentBookingChainAsync(booking, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DateTime.Today, DateTime.Today.AddDays(3)));

            await _useCase.ExecuteAsync(bookingId, isFined: false, CancellationToken.None);

            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            _unitOfWorkMock.Verify(x => x.Bookings.Update(booking), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<BookingUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _notificationServiceMock.Verify(x => x.NotifyUserAboutBookingCancellationAsync(booking, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingHasPayment_RefundsAndCancelsBooking()
        {
            var paymentId = Guid.NewGuid();
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                HousingId = Guid.NewGuid(),
                Status = BookingStatus.Confirmed,
                Payment = new Payment { PaymentId = paymentId }
            };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(booking.BookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            _unitOfWorkMock.Setup(x => x.Bookings.GetCurrentBookingChainAsync(booking, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DateTime.Today, DateTime.Today.AddDays(2)));

            await _useCase.ExecuteAsync(booking.BookingId, isFined: true, CancellationToken.None);

            _refundPaymentUseCaseMock.Verify(x => x.ExecuteAsync(paymentId, true, It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<BookingUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _notificationServiceMock.Verify(x => x.NotifyUserAboutBookingCancellationAsync(booking, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingNotFound_ThrowsNotFoundException()
        {
            var bookingId = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _useCase.ExecuteAsync(bookingId, isFined: false, CancellationToken.None));
        }
    }
}

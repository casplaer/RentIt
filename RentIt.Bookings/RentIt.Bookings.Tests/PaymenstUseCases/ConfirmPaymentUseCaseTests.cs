using Moq;
using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.UseCases.Payments;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Tests.PaymenstUseCases
{
    public class ConfirmPaymentUseCaseTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IBookingNotificationService> _bookingNotificationServiceMock;
        private readonly ConfirmPaymentUseCase _useCase;

        public ConfirmPaymentUseCaseTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger>();
            _bookingNotificationServiceMock = new Mock<IBookingNotificationService>();
            _useCase = new ConfirmPaymentUseCase(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _bookingNotificationServiceMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingAndPaymentExists_ConfirmsPayment()
        {
            var userId = Guid.NewGuid().ToString();
            var userGuid = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();

            var booking = new Booking
            {
                BookingId = bookingId,
                UserId = userGuid,
                Status = BookingStatus.Pending,
                Payment = new Payment { PaymentId = paymentId, Status = PaymentStatus.Pending }
            };

            var payment = new Payment
            {
                PaymentId = paymentId,
                Status = PaymentStatus.Pending
            };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            _unitOfWorkMock.Setup(x => x.Payments.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            await _useCase.ExecuteAsync(bookingId, userGuid.ToString(), CancellationToken.None);

            Assert.Equal(PaymentStatus.Completed, payment.Status);
            Assert.Equal(BookingStatus.Paid, booking.Status);
            _unitOfWorkMock.Verify(u => u.Payments.Update(payment), Times.Once);
            _unitOfWorkMock.Verify(u => u.Bookings.Update(booking), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _bookingNotificationServiceMock.Verify(s => s.NotifyUserAboutPaymentSuccessAsync(booking, payment, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIdFormatIsInvalid_ThrowsArgumentException()
        {
            var invalidUserId = "invalid-guid";
            var bookingId = Guid.NewGuid();

            await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(bookingId, invalidUserId, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingNotFound_ThrowsNotFoundException()
        {
            var userId = Guid.NewGuid().ToString();
            var bookingId = Guid.NewGuid();

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(bookingId, userId, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserTriesToPayForOtherUserBooking_ThrowsArgumentException()
        {
            var userId = Guid.NewGuid().ToString();
            var otherUserGuid = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();

            var booking = new Booking
            {
                BookingId = bookingId,
                UserId = otherUserGuid,
                Status = BookingStatus.Pending,
                Payment = new Payment { PaymentId = paymentId, Status = PaymentStatus.Pending }
            };

            var payment = new Payment
            {
                PaymentId = paymentId,
                Status = PaymentStatus.Pending
            };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            _unitOfWorkMock.Setup(x => x.Payments.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(bookingId, userId, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenPaymentNotFound_ThrowsException()
        {
            var userId = Guid.NewGuid().ToString();
            var bookingId = Guid.NewGuid();

            var booking = new Booking
            {
                BookingId = bookingId,
                UserId = Guid.NewGuid(),
                Status = BookingStatus.Pending,
                Payment = new Payment { PaymentId = Guid.NewGuid(), Status = PaymentStatus.Pending }
            };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            _unitOfWorkMock.Setup(x => x.Payments.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Payment)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(bookingId, userId, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingCancelled_ThrowsArgumentException()
        {
            var userId = Guid.NewGuid().ToString();
            var bookingId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();

            var booking = new Booking
            {
                BookingId = bookingId,
                UserId = Guid.NewGuid(),
                Status = BookingStatus.Cancelled,
                Payment = new Payment { PaymentId = paymentId, Status = PaymentStatus.Pending }
            };

            var payment = new Payment
            {
                PaymentId = paymentId,
                Status = PaymentStatus.Pending
            };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            _unitOfWorkMock.Setup(x => x.Payments.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(bookingId, userId, CancellationToken.None));
        }
    }
}
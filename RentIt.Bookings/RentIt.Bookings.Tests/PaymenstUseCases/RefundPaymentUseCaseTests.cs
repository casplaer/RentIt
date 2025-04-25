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
    public class RefundPaymentUseCaseTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IBookingNotificationService> _bookingNotificationServiceMock;
        private readonly RefundPaymentUseCase _useCase;

        public RefundPaymentUseCaseTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger>();
            _bookingNotificationServiceMock = new Mock<IBookingNotificationService>();
            _useCase = new RefundPaymentUseCase(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _bookingNotificationServiceMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenPaymentExists_RefundsPayment()
        {
            var paymentId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var payment = new Payment
            {
                PaymentId = paymentId,
                BookingId = bookingId,
                Amount = 100.0m,
                Status = PaymentStatus.Completed
            };

            var booking = new Booking
            {
                BookingId = bookingId,
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(2),
                Status = BookingStatus.Pending
            };

            _unitOfWorkMock.Setup(x => x.Payments.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);
            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            await _useCase.ExecuteAsync(paymentId, isFined: false, CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.Payments.Update(payment), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _bookingNotificationServiceMock.Verify(s => s.NotifyUserAboutRefundAsync(booking, payment, It.IsAny<bool>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenPaymentNotFound_ThrowsException()
        {
            var paymentId = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.Payments.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Payment)null);

            await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync(paymentId, isFined: false, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingNotFound_ThrowsNotFoundException()
        {
            var paymentId = Guid.NewGuid();
            var payment = new Payment
            {
                PaymentId = paymentId,
                BookingId = Guid.NewGuid(),
                Amount = 100.0m,
                Status = PaymentStatus.Completed
            };

            _unitOfWorkMock.Setup(x => x.Payments.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);
            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(paymentId, isFined: false, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIsFined_RefundsWithFine()
        {
            var paymentId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var payment = new Payment
            {
                PaymentId = paymentId,
                BookingId = bookingId,
                Amount = 100.0m,
                Status = PaymentStatus.Completed
            };

            var booking = new Booking
            {
                BookingId = bookingId,
                StartDate = DateTime.UtcNow.AddDays(-5),
                EndDate = DateTime.UtcNow.AddDays(2),
                Status = BookingStatus.Pending
            };

            _unitOfWorkMock.Setup(x => x.Payments.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);
            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            await _useCase.ExecuteAsync(paymentId, isFined: true, CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.Payments.Update(payment), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _bookingNotificationServiceMock.Verify(s => s.NotifyUserAboutRefundAsync(booking, payment, true, It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
using Moq;
using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.UseCases.Payments;
using RentIt.Bookings.Contracts.Requests.Payments;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Tests.PaymenstUseCases
{
    public class CreatePaymentUseCaseTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IBookingNotificationService> _bookingNotificationServiceMock;
        private readonly CreatePaymentUseCase _useCase;

        public CreatePaymentUseCaseTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger>();
            _bookingNotificationServiceMock = new Mock<IBookingNotificationService>();
            _useCase = new CreatePaymentUseCase(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _bookingNotificationServiceMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingExists_CreatesPaymentAndSaves()
        {
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var amount = 100.0m;

            var booking = new Booking
            {
                BookingId = bookingId,
                UserId = userId,
                Status = BookingStatus.Pending
            };

            var request = new ProcessTestPaymentRequest
            (
                bookingId,
                amount
            );

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                BookingId = bookingId,
                Amount = amount,
                PaymentTime = DateTime.UtcNow,
                Status = PaymentStatus.Pending
            };

            _unitOfWorkMock.Setup(x => x.Payments.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _useCase.ExecuteAsync(request, CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.Payments.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _bookingNotificationServiceMock.Verify(s => s.NotifyUserAboutBookingConfirmationAsync(booking, It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingNotFound_ThrowsNotFoundException()
        {
            var request = new ProcessTestPaymentRequest
            (
                Guid.NewGuid(),
                100.0m
            );

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(request.BookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenPaymentCreated_SuccessfulNotification()
        {
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var amount = 100.0m;

            var booking = new Booking
            {
                BookingId = bookingId,
                UserId = userId,
                Status = BookingStatus.Pending
            };

            var request = new ProcessTestPaymentRequest
            (
                bookingId,
                amount
            );

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                BookingId = bookingId,
                Amount = amount,
                PaymentTime = DateTime.UtcNow,
                Status = PaymentStatus.Pending
            };

            _unitOfWorkMock.Setup(x => x.Payments.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _useCase.ExecuteAsync(request, CancellationToken.None);

            _bookingNotificationServiceMock.Verify(s => s.NotifyUserAboutBookingConfirmationAsync(booking, It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
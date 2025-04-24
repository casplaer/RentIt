using Moq;
using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.EventBus;
using RentIt.Bookings.Application.Interfaces.Services.Grpc;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using RentIt.Bookings.Application.UseCases.Bookings;
using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Contracts.Requests.Payments;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.MessageBroker.Contracts.Events;
using Serilog;

namespace RentIt.Bookings.Tests.BookingUseCases
{
    public class ConfirmBookingUseCaseTests
    {
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IHousingIntegrationService> _housingServiceMock = new();
        private readonly Mock<ICreatePaymentUseCase> _createPaymentUseCaseMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IEventBus> _eventBusMock = new();

        private readonly ConfirmBookingUseCase _useCase;

        public ConfirmBookingUseCaseTests()
        {
            _useCase = new ConfirmBookingUseCase(
                _loggerMock.Object,
                _housingServiceMock.Object,
                _createPaymentUseCaseMock.Object,
                _unitOfWorkMock.Object,
                _eventBusMock.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingIsValid_ConfirmsSuccessfully()
        {
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var housingId = Guid.NewGuid();
            var totalPrice = 100m;

            var booking = new Booking
            {
                BookingId = bookingId,
                HousingId = housingId,
                UserId = Guid.NewGuid(),
                Status = BookingStatus.Pending,
                TotalPrice = totalPrice
            };

            var payment = new Payment { PaymentId = Guid.NewGuid() };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            _housingServiceMock.Setup(x => x.GetHousingInfoAsync(housingId))
                .ReturnsAsync(new HousingInfoDto ( userId, "test", 10m ));
            _createPaymentUseCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ProcessTestPaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);
            _unitOfWorkMock.Setup(x => x.Bookings.GetCurrentBookingChainAsync(housingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DateTime.Today, DateTime.Today.AddDays(1)));

            await _useCase.ExecuteAsync(userId.ToString(), bookingId, CancellationToken.None);

            Assert.Equal(BookingStatus.Confirmed, booking.Status);
            Assert.Equal(payment, booking.Payment);
            _unitOfWorkMock.Verify(x => x.Bookings.Update(booking), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<BookingUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIdIsInvalid_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _useCase.ExecuteAsync("invalid", Guid.NewGuid(), CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingNotFound_ThrowsNotFoundException()
        {
            var bookingId = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _useCase.ExecuteAsync(Guid.NewGuid().ToString(), bookingId, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingStatusIsNotPending_ThrowsNotFoundException()
        {
            var bookingId = Guid.NewGuid();
            var booking = new Booking
            {
                BookingId = bookingId,
                Status = BookingStatus.Confirmed
            };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _useCase.ExecuteAsync(Guid.NewGuid().ToString(), bookingId, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIsNotOwner_ThrowsArgumentException()
        {
            var bookingId = Guid.NewGuid();
            var housingId = Guid.NewGuid();

            var booking = new Booking
            {
                BookingId = bookingId,
                HousingId = housingId,
                Status = BookingStatus.Pending
            };

            _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            _housingServiceMock.Setup(x => x.GetHousingInfoAsync(housingId))
                .ReturnsAsync(new HousingInfoDto ( Guid.NewGuid(), "test", 10 ));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _useCase.ExecuteAsync(Guid.NewGuid().ToString(), bookingId, CancellationToken.None));
        }
    }
}

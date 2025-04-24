using AutoMapper;
using FluentValidation;
using Moq;
using RentIt.Bookings.Application.Services;
using RentIt.Bookings.Application.Services.Grpc;
using RentIt.Bookings.Application.UseCases.Bookings;
using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Tests
{
    public class AddBookingUseCaseTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<HousingIntegrationService> _housingServiceMock = new();
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IValidator<CreateBookingRequest>> _validatorMock = new();
        private readonly Mock<BookingNotificationService> _notificationServiceMock = new();

        private readonly AddBookingUseCase _useCase;

        public AddBookingUseCaseTests()
        {
            _useCase = new AddBookingUseCase(
                _unitOfWorkMock.Object,
                _housingServiceMock.Object,
                _loggerMock.Object,
                _mapperMock.Object,
                _validatorMock.Object,
                _notificationServiceMock.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_InvalidUserId_ThrowsArgumentException()
        {
            var request = new CreateBookingRequest(Guid.NewGuid(), DateTime.Today, DateTime.Today.AddDays(2));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _useCase.ExecuteAsync(request, "not-a-guid", CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_UserIsOwner_ThrowsArgumentException()
        {
            var housingId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var request = new CreateBookingRequest(housingId, DateTime.Today, DateTime.Today.AddDays(3));

            _housingServiceMock.Setup(x => x.GetHousingInfoAsync(housingId))
                .ReturnsAsync(new HousingInfoDto
                (
                    ownerId,
                    "Housing",
                    100m
                ));

            var userId = ownerId.ToString();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _useCase.ExecuteAsync(request, userId, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_ValidInput_CreatesBooking()
        {
            var housingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateBookingRequest(housingId, DateTime.Today, DateTime.Today.AddDays(2));

            _housingServiceMock.Setup(x => x.GetHousingInfoAsync(housingId))
                .ReturnsAsync(new HousingInfoDto
                (
                    Guid.NewGuid(),
                    "Housing",
                    150m
                ));

            var booking = new Booking();

            _mapperMock.Setup(m => m.Map<Booking>(It.IsAny<CreateBookingRequest>(), It.IsAny<Action<IMappingOperationOptions>>()))
                .Returns(booking);

            _validatorMock.Setup(v => v.ValidateAndThrowAsync(request, default))
                .Returns(Task.CompletedTask);

            var result = await _useCase.ExecuteAsync(request, userId.ToString(), CancellationToken.None);

            _unitOfWorkMock.Verify(u => u.Bookings.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notificationServiceMock.Verify(n => n.NotifyOwnerAboutNewBookingAsync(It.IsAny<HousingInfoDto>(), request, It.IsAny<CancellationToken>()), Times.Once);
            _notificationServiceMock.Verify(n => n.NotifyUserAboutBookingCreationAsync(It.IsAny<HousingInfoDto>(), request, userId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

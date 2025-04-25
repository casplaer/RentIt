using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.Services.Grpc;
using RentIt.Bookings.Application.Mappings.Bookings;
using RentIt.Bookings.Application.UseCases.Bookings;
using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;
using Xunit;

namespace RentIt.Bookings.Tests.BookingUseCases
{
    public class AddBookingUseCaseTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IHousingIntegrationService> _housingServiceMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IValidator<CreateBookingRequest>> _validatorMock;
        private readonly Mock<IBookingNotificationService> _notificationServiceMock;
        private readonly IMapper _mapper;
        private readonly AddBookingUseCase _useCase;

        public AddBookingUseCaseTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _housingServiceMock = new Mock<IHousingIntegrationService>();
            _loggerMock = new Mock<ILogger>();
            _validatorMock = new Mock<IValidator<CreateBookingRequest>>();
            _notificationServiceMock = new Mock<IBookingNotificationService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CreateBookingProfile>();
            });

            _mapper = config.CreateMapper();

            _useCase = new AddBookingUseCase(
                _unitOfWorkMock.Object,
                _housingServiceMock.Object,
                _loggerMock.Object,
                _mapper,
                _validatorMock.Object,
                _notificationServiceMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIdIsInvalid_ThrowsArgumentException()
        {
            var invalidUserId = "invalid-guid";
            var request = new CreateBookingRequest(Guid.NewGuid(), DateTime.Now, DateTime.Now.AddDays(1));

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _useCase.ExecuteAsync(request, invalidUserId, CancellationToken.None));

            Assert.Equal("Некорректный формат ID.", exception.Message);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserTriesToBookOwnHousing_ThrowsArgumentException()
        {
            var userId = Guid.NewGuid().ToString();
            var userGuid = Guid.Parse(userId);
            var housingId = Guid.NewGuid();
            var request = new CreateBookingRequest(housingId, DateTime.Now, DateTime.Now.AddDays(1));

            var housingInfo = new HousingInfoDto(userGuid, "Housing Name", 100);
            _housingServiceMock.Setup(h => h.GetHousingInfoAsync(housingId)).ReturnsAsync(housingInfo);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _useCase.ExecuteAsync(request, userId, CancellationToken.None));

            Assert.Equal("Извините, но забронировать свою же собственность невозможно.", exception.Message);
        }

        [Fact]
        public async Task ExecuteAsync_WhenValidRequest_CreatesBookingSuccessfully()
        {
            var userId = Guid.NewGuid().ToString();
            var userGuid = Guid.Parse(userId);
            var housingId = Guid.NewGuid();
            var request = new CreateBookingRequest(housingId, DateTime.Now, DateTime.Now.AddDays(1));

            var housingInfo = new HousingInfoDto(Guid.NewGuid(), "Housing Name", 100m);
            _housingServiceMock.Setup(h => h.GetHousingInfoAsync(housingId)).ReturnsAsync(housingInfo);
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateBookingRequest>(), CancellationToken.None))
                          .ReturnsAsync(new ValidationResult());

            var bookingRepositoryMock = new Mock<IBookingRepository>();
            bookingRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Booking>(), CancellationToken.None))
                                 .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.Bookings).Returns(bookingRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(CancellationToken.None))
                           .ReturnsAsync(1);

            var nights = (request.EndDate.Date - request.StartDate.Date).Days;
            var expectedTotalPrice = housingInfo.PricePerNight * nights;

            var mappedBooking = _mapper.Map<Booking>(request, opts =>
            {
                opts.Items["UserId"] = userGuid;
                opts.Items["ComputedTotalPrice"] = expectedTotalPrice;
            });

            var result = await _useCase.ExecuteAsync(request, userId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(housingId, result.HousingId);
            Assert.Equal(userGuid, result.UserId);
            Assert.Equal(request.StartDate, result.StartDate);
            Assert.Equal(request.EndDate, result.EndDate);
            Assert.Equal(expectedTotalPrice, result.TotalPrice);
            Assert.Equal(BookingStatus.Pending, result.Status);

            bookingRepositoryMock.Verify(r => r.AddAsync(It.Is<Booking>(b =>
                b.HousingId == housingId &&
                b.UserId == userGuid &&
                b.StartDate == request.StartDate &&
                b.EndDate == request.EndDate &&
                b.TotalPrice == expectedTotalPrice &&
                b.Status == BookingStatus.Pending), CancellationToken.None), Times.Once);

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenBookingCreated_NotificationsSent()
        {
            var userId = Guid.NewGuid().ToString();
            var userGuid = Guid.Parse(userId);
            var housingId = Guid.NewGuid();
            var request = new CreateBookingRequest(housingId, DateTime.Now, DateTime.Now.AddDays(1));

            var housingInfo = new HousingInfoDto(Guid.NewGuid(), "Housing Name", 100m);
            _housingServiceMock.Setup(h => h.GetHousingInfoAsync(housingId)).ReturnsAsync(housingInfo);

            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateBookingRequest>(), CancellationToken.None))
                          .ReturnsAsync(new ValidationResult());

            _notificationServiceMock.Setup(n => n.NotifyOwnerAboutNewBookingAsync(housingInfo, request, CancellationToken.None))
                                   .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(n => n.NotifyUserAboutBookingCreationAsync(housingInfo, request, userGuid, CancellationToken.None))
                                   .Returns(Task.CompletedTask);

            var bookingRepositoryMock = new Mock<IBookingRepository>();
            bookingRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Booking>(), CancellationToken.None))
                                 .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.Bookings).Returns(bookingRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(CancellationToken.None))
                           .ReturnsAsync(1);

            var mappedBooking = _mapper.Map<Booking>(request, opts =>
            {
                opts.Items["UserId"] = userGuid;
                opts.Items["ComputedTotalPrice"] = 100m * (request.EndDate.Date - request.StartDate.Date).Days;
            });
            var result = await _useCase.ExecuteAsync(request, userId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(housingId, result.HousingId);
            Assert.Equal(userGuid, result.UserId);
            Assert.Equal(BookingStatus.Pending, result.Status);
            _notificationServiceMock.Verify(n => n.NotifyOwnerAboutNewBookingAsync(housingInfo, request, CancellationToken.None), Times.Once);
            _notificationServiceMock.Verify(n => n.NotifyUserAboutBookingCreationAsync(housingInfo, request, userGuid, CancellationToken.None), Times.Once);
            _unitOfWorkMock.Verify(u => u.Bookings.AddAsync(It.IsAny<Booking>(), CancellationToken.None), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
        }
    }
}
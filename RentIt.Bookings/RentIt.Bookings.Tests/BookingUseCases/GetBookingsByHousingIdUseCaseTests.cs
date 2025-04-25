using AutoMapper;
using Moq;
using RentIt.Bookings.Application.Interfaces.Services.Grpc;
using RentIt.Bookings.Application.UseCases.Bookings;
using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Core.Interfaces.Specitfications;
using Serilog;

namespace RentIt.Bookings.Tests.BookingUseCases
{
    public class GetBookingsByHousingIdUseCaseTests
    {
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IHousingIntegrationService> _housingServiceMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly GetBookingsByHousingIdUseCase _useCase;

        public GetBookingsByHousingIdUseCaseTests()
        {
            _useCase = new GetBookingsByHousingIdUseCase(
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _housingServiceMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIsOwner_ReturnsBookings()
        {
            var housingId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var request = new GetBookingsByPagesRequest (1, 10);

            var bookings = new PaginatedResult<Booking>
            {
                Items = new List<Booking>(),
                TotalCount = 0,
                TotalPages = 0,
                CurrentPage = 1,
                PageSize = 10
            };

            var bookingsDto = new PaginatedResult<BookingDto>
            {
                Items = new List<BookingDto>(),
                TotalCount = 0,
                TotalPages = 0,
                CurrentPage = 1,
                PageSize = 10
            };

            _housingServiceMock.Setup(x => x.GetHousingInfoAsync(housingId))
                .ReturnsAsync(new HousingInfoDto(Guid.Parse(userId), "Test Housing", 100));

            _unitOfWorkMock.Setup(x => x.Bookings.GetPaginatedFilteredBookingsAsync(
                    It.IsAny<ISpecification<Booking>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            _mapperMock.Setup(x => x.Map<PaginatedResult<BookingDto>>(bookings))
                .Returns(bookingsDto);

            var result = await _useCase.ExecuteAsync(housingId, userId, "User", request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIsAdmin_ReturnsBookings()
        {
            var housingId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var request = new GetBookingsByPagesRequest (1, 10);

            var bookings = new PaginatedResult<Booking>
            {
                Items = new List<Booking>(),
                TotalCount = 0,
                TotalPages = 0,
                CurrentPage = 1,
                PageSize = 10
            };

            var bookingsDto = new PaginatedResult<BookingDto>
            {
                Items = new List<BookingDto>(),
                TotalCount = 0,
                TotalPages = 0,
                CurrentPage = 1,
                PageSize = 10
            };

            _housingServiceMock.Setup(x => x.GetHousingInfoAsync(housingId))
                .ReturnsAsync(new HousingInfoDto(Guid.NewGuid(), "Admin Housing", 150));

            _unitOfWorkMock.Setup(x => x.Bookings.GetPaginatedFilteredBookingsAsync(
                    It.IsAny<ISpecification<Booking>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            _mapperMock.Setup(x => x.Map<PaginatedResult<BookingDto>>(bookings))
                .Returns(bookingsDto);

            var result = await _useCase.ExecuteAsync(housingId, userId, "Admin", request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIsNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            var housingId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var request = new GetBookingsByPagesRequest (1, 10);

            _housingServiceMock.Setup(x => x.GetHousingInfoAsync(housingId))
                .ReturnsAsync(new HousingInfoDto(Guid.NewGuid(), "Other Housing", 200));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _useCase.ExecuteAsync(housingId, userId, "User", request, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIdIsInvalid_ThrowsArgumentException()
        {
            var housingId = Guid.NewGuid();
            var invalidUserId = "not-a-guid";
            var request = new GetBookingsByPagesRequest (1, 10);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _useCase.ExecuteAsync(housingId, invalidUserId, "User", request, CancellationToken.None));
        }
    }
}
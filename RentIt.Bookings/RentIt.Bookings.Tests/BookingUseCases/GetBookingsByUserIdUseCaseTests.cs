using AutoMapper;
using Moq;
using RentIt.Bookings.Application.UseCases.Bookings;
using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Core.Interfaces.Specitfications;
using Serilog;

namespace RentIt.Bookings.Tests.BookingUseCases
{
    public class GetBookingsByUserIdUseCaseTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly GetBookingsByUserIdUseCase _useCase;

        public GetBookingsByUserIdUseCaseTests()
        {
            _useCase = new GetBookingsByUserIdUseCase(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIsAuthorized_ReturnsBookings()
        {
            var userId = Guid.NewGuid();
            var request = new GetBookingsByPagesRequest(1, 10);

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

            _unitOfWorkMock.Setup(x => x.Bookings.GetPaginatedFilteredBookingsAsync(
                It.IsAny<ISpecification<Booking>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            _mapperMock.Setup(x => x.Map<PaginatedResult<BookingDto>>(bookings))
                .Returns(bookingsDto);

            var result = await _useCase.ExecuteAsync(userId, userId.ToString(), "User", request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIsAdmin_ReturnsBookings()
        {
            var userId = Guid.NewGuid();
            var request = new GetBookingsByPagesRequest(1, 10);

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

            _unitOfWorkMock.Setup(x => x.Bookings.GetPaginatedFilteredBookingsAsync(
                It.IsAny<ISpecification<Booking>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            _mapperMock.Setup(x => x.Map<PaginatedResult<BookingDto>>(bookings))
                .Returns(bookingsDto);

            var result = await _useCase.ExecuteAsync(userId, Guid.NewGuid().ToString(), "Admin", request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserIsNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            var userId = Guid.NewGuid();
            var request = new GetBookingsByPagesRequest(1, 10);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _useCase.ExecuteAsync(userId, Guid.NewGuid().ToString(), "User", request, CancellationToken.None));
        }
    }
}
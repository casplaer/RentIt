using AutoMapper;
using FluentValidation;
using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Contracts.Dto.Users;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Exceptions;
using RentIt.Housing.Domain.Services.Interfaces;
using Serilog;

namespace RentIt.Housing.Tests.HousingService
{
    public class GetByIdAsyncTests
    {
        private readonly Mock<IHousingRepository> _housingRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IUserIntegrationService> _userIntegrationServiceMock = new();
        private readonly Mock<IBookingIntegrationService> _bookingIntegrationServiceMock = new();
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IHousingImageService> _imageServiceMock = new();
        private readonly Mock<ISpamProfanityFilterService> _filterServiceMock = new();
        private readonly Mock<IEventBus> _eventBusMock = new();

        private readonly Domain.Services.HousingService _housingService;

        public GetByIdAsyncTests()
        {
            _housingService = new Domain.Services.HousingService(
                _housingRepoMock.Object,
                _mapperMock.Object,
                It.IsAny<IValidator<CreateHousingRequest>>(),
                It.IsAny<IValidator<GetFilteredHousingsRequest>>(),
                It.IsAny<IValidator<UpdateHousingRequest>>(),
                _imageServiceMock.Object,
                _userIntegrationServiceMock.Object,
                _bookingIntegrationServiceMock.Object,
                _filterServiceMock.Object,
                _loggerMock.Object,
                _eventBusMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnHousing_WhenHousingExists()
        {
            var housingId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var housing = new HousingEntity { HousingId = housingId, OwnerId = ownerId };
            var userInfo = new UserInfoDto("John", "Doe", "john.doe@example.com", "+123456789");

            _housingRepoMock.Setup(r => r.GetByIdAsync(housingId, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(housing);

            _userIntegrationServiceMock.Setup(s => s.GetUserInfoAsync(ownerId))
                                       .ReturnsAsync(userInfo);

            var result = await _housingService.GetByIdAsync(housingId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(housingId, result.Housing.HousingId);
            Assert.Equal("john.doe@example.com", result.UserInfoDto.Email);

            _housingRepoMock.Verify(r => r.GetByIdAsync(housingId, It.IsAny<CancellationToken>()), Times.Once);
            _userIntegrationServiceMock.Verify(s => s.GetUserInfoAsync(ownerId), Times.Once);
        }


        [Fact]
        public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenHousingDoesNotExist()
        {
            var housingId = Guid.NewGuid();

            _housingRepoMock.Setup(r => r.GetByIdAsync(housingId, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((HousingEntity?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _housingService.GetByIdAsync(housingId, CancellationToken.None));

            ex.Message.Equals("Собственности с таким ID не найдено.");

            _housingRepoMock.Verify(r => r.GetByIdAsync(housingId, It.IsAny<CancellationToken>()), Times.Once);
            _userIntegrationServiceMock.Verify(s => s.GetUserInfoAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldLog_WhenHousingIsFetched()
        {
            var housingId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var housing = new HousingEntity 
            { 
                HousingId = housingId, 
                OwnerId = ownerId 
            };

            var userInfo = new UserInfoDto("Jane", "Smith", "jane.smith@example.com", "+987654321");

            _housingRepoMock.Setup(r => r.GetByIdAsync(housingId, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(housing);

            _userIntegrationServiceMock.Setup(s => s.GetUserInfoAsync(ownerId))
                                       .ReturnsAsync(userInfo);

            await _housingService.GetByIdAsync(housingId, CancellationToken.None);

            _loggerMock.Verify(x => x.Information("Получение собственности с ID {HousingId}", housingId), Times.Once);
            _loggerMock.Verify(x => x.Information("Собственность с ID {HousingId} успешно получена", housingId), Times.Once);
        }
    }
}
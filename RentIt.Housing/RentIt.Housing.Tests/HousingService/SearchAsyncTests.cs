using AutoMapper;
using FluentValidation;
using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Enums;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.DataAccess.Specifications.Housing;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Services.Interfaces;
using Serilog;

namespace RentIt.Housing.Tests.HousingService
{
    public class SearchAsyncTests
    {
        private readonly Mock<IHousingRepository> _housingRepositoryMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IValidator<GetFilteredHousingsRequest>> _validatorMock = new();
        private readonly Mock<IHousingImageService> _imageServiceMock = new();
        private readonly Mock<IUserIntegrationService> _userIntegrationServiceMock = new();
        private readonly Mock<IBookingIntegrationService> _bookingIntegrationServiceMock = new();
        private readonly Mock<ISpamProfanityFilterService> _filterServiceMock = new();
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IEventBus> _eventBusMock = new();

        private readonly Domain.Services.HousingService _housingService;

        public SearchAsyncTests()
        {
            _housingService = new Domain.Services.HousingService(
                _housingRepositoryMock.Object,
                _mapperMock.Object,
                Mock.Of<IValidator<CreateHousingRequest>>(),
                _validatorMock.Object,
                Mock.Of<IValidator<UpdateHousingRequest>>(),
                _imageServiceMock.Object,
                _userIntegrationServiceMock.Object,
                _bookingIntegrationServiceMock.Object,
                _filterServiceMock.Object,
                _loggerMock.Object,
                _eventBusMock.Object
            );
        }


        [Fact]
        public async Task ReturnsHousingList_WhenRequestIsValid()
        {
            var request = new GetFilteredHousingsRequest(
                Title: "title",
                Address: "address",
                City: "city",
                Country: "country",
                PricePerNight: 100,
                NumberOfRooms: 2,
                Rating: 4.5,
                Status: HousingStatus.Available,
                UserStartDate: DateTime.UtcNow,
                UserEndDate: DateTime.UtcNow.AddDays(7),
                Page: 1,
                PageSize: 10
            );

            var housings = new List<HousingEntity> { new HousingEntity() };

            _validatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _housingRepositoryMock
                .Setup(r => r.SearchAsync(It.IsAny<SearchHousingSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(housings);

            var result = await _housingService.SearchAsync(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task ReturnsEmptyList_WhenNothingMatches()
        {
            var request = new GetFilteredHousingsRequest(
                Title: "nothing",
                Address: null,
                City: null,
                Country: null,
                PricePerNight: null,
                NumberOfRooms: null,
                Rating: null,
                Status: null,
                UserStartDate: null,
                UserEndDate: null,
                Page: 1,
                PageSize: 10
            );

            _validatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _housingRepositoryMock
                .Setup(r => r.SearchAsync(It.IsAny<SearchHousingSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HousingEntity>());

            var result = await _housingService.SearchAsync(request, CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ReturnsAllHousings_WhenAllFieldsAreNull()
        {
            var request = new GetFilteredHousingsRequest(
                Title: null,
                Address: null,
                City: null,
                Country: null,
                PricePerNight: null,
                NumberOfRooms: null,
                Rating: null,
                Status: null,
                UserStartDate: null,
                UserEndDate: null,
                Page: 1,
                PageSize: 10
            );

            var housings = new List<HousingEntity>
            {
                new HousingEntity(),
                new HousingEntity()
            };

            _validatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _housingRepositoryMock
                .Setup(r => r.SearchAsync(It.IsAny<SearchHousingSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(housings);

            var result = await _housingService.SearchAsync(request, CancellationToken.None);

            Assert.Equal(2, result.Count());
        }
    }
}
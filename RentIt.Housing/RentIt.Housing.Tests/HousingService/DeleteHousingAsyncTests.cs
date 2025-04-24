using AutoMapper;
using FluentValidation;
using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Exceptions;
using RentIt.Housing.Domain.Services.Interfaces;
using Serilog;

namespace RentIt.Housing.Tests.HousingService
{
    public class DeleteHousingAsyncTests
    {
        private readonly Mock<IHousingRepository> _housingRepository;
        private readonly Mock<IHousingImageService> _imageService;
        private readonly Mock<IBookingIntegrationService> _bookingIntegrationService;
        private readonly Mock<ILogger> _logger;
        private readonly Domain.Services.HousingService _service;

        public DeleteHousingAsyncTests()
        {
            _housingRepository = new Mock<IHousingRepository>();
            _imageService = new Mock<IHousingImageService>();
            _bookingIntegrationService = new Mock<IBookingIntegrationService>();
            _logger = new Mock<ILogger>();

            _service = new Domain.Services.HousingService(
                _housingRepository.Object,
                new Mock<IMapper>().Object,
                new Mock<IValidator<CreateHousingRequest>>().Object,
                new Mock<IValidator<GetFilteredHousingsRequest>>().Object,
                new Mock<IValidator<UpdateHousingRequest>>().Object,
                _imageService.Object,
                new Mock<IUserIntegrationService>().Object,
                _bookingIntegrationService.Object,
                new Mock<ISpamProfanityFilterService>().Object,
                _logger.Object,
                new Mock<IEventBus>().Object
            );
        }

        [Fact]
        public async Task DeletesHousingSuccessfully_WhenNoBookingsExist()
        {
            var housingId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var housing = new HousingEntity
            {
                HousingId = housingId,
                OwnerId = Guid.Parse(userId),
                Images = new List<HousingImage>()
            };

            _housingRepository.Setup(r => r.GetByIdAsync(housingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(housing);

            _bookingIntegrationService.Setup(b => b.GetExistBookings(housingId))
                .ReturnsAsync(false);

            _imageService.Setup(i => i.ClearImagesAsync(housing.Images, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _housingRepository.Setup(r => r.DeleteAsync(housingId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _service.DeleteHousingAsync(housingId, userId, CancellationToken.None);

            _housingRepository.Verify(r => r.DeleteAsync(housingId, It.IsAny<CancellationToken>()), Times.Once);
            _imageService.Verify(i => i.ClearImagesAsync(housing.Images, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ThrowsNotFoundException_WhenHousingDoesNotExist()
        {
            var housingId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            _housingRepository.Setup(r => r.GetByIdAsync(housingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((HousingEntity?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DeleteHousingAsync(housingId, userId, CancellationToken.None));
        }

        [Fact]
        public async Task ThrowsArgumentException_WhenBookingsExist()
        {
            var housingId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var housing = new HousingEntity
            {
                HousingId = housingId,
                OwnerId = Guid.Parse(userId),
                Images = new List<HousingImage>()
            };

            _housingRepository.Setup(r => r.GetByIdAsync(housingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(housing);

            _bookingIntegrationService.Setup(b => b.GetExistBookings(housingId))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteHousingAsync(housingId, userId, CancellationToken.None));
        }

        [Fact]
        public async Task ThrowsUnauthorizedAccessException_WhenUserIsNotOwner()
        {
            var housingId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var housing = new HousingEntity
            {
                HousingId = housingId,
                OwnerId = Guid.NewGuid(),
                Images = new List<HousingImage>()
            };

            _housingRepository.Setup(r => r.GetByIdAsync(housingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(housing);

            _bookingIntegrationService.Setup(b => b.GetExistBookings(housingId))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.DeleteHousingAsync(housingId, userId, CancellationToken.None));
        }
    }

}

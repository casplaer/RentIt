using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Mappings.Housing;
using RentIt.Housing.Domain.Services.Interfaces;

namespace RentIt.Housing.Tests.HousingService
{
    public class AddHousingAsyncTests
    {
        private readonly Mock<IHousingRepository> _housingRepository = new();
        private readonly Mock<IHousingImageService> _imageService = new();
        private readonly Mock<IValidator<CreateHousingRequest>> _validator = new();
        private readonly IMapper _mapper;

        private readonly Domain.Services.HousingService _service;

        public AddHousingAsyncTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<HousingProfile>();
            });
            _mapper = config.CreateMapper();

            _service = new Domain.Services.HousingService(
                _housingRepository.Object,
                _mapper,
                _validator.Object,
                new Mock<IValidator<GetFilteredHousingsRequest>>().Object,
                new Mock<IValidator<UpdateHousingRequest>>().Object,
                _imageService.Object,
                new Mock<IUserIntegrationService>().Object,
                new Mock<IBookingIntegrationService>().Object,
                new Mock<ISpamProfanityFilterService>().Object,
                new Mock<Serilog.ILogger>().Object,
                new Mock<IEventBus>().Object
            );
        }

        private CreateHousingRequest BuildRequest(IEnumerable<IFormFile>? images = null)
        {
            return new(
                Title: "Test Housing",
                Description: "Test Description",
                Country: "Test Country",
                City: "Test City",
                Address: "Test Address",
                PricePerNight: 100,
                NumberOfRooms: 2,
                Amenities: ["WiFi", "TV"],
                Images: images
            );
        }

        [Fact]
        public async Task AddsHousingSuccessfully_WhenRequestIsValid_WithoutImages()
        {
            var ownerId = Guid.NewGuid().ToString();
            var request = BuildRequest(null);

            _validator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

            _housingRepository.Setup(r => r.AddAsync(It.IsAny<HousingEntity>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

            await _service.AddHousingAsync(ownerId, request, CancellationToken.None);

            _housingRepository.Verify(r => r.AddAsync(It.IsAny<HousingEntity>(), It.IsAny<CancellationToken>()), Times.Once);
            _imageService.Verify(i => i.UploadImagesAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<IFormFile>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AddsHousingSuccessfully_WhenRequestIsValid_WithImages()
        {
            var ownerId = Guid.NewGuid().ToString();
            var images = new List<IFormFile> { Mock.Of<IFormFile>() };
            var request = BuildRequest(images);

            _validator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

            _imageService.Setup(i => i.UploadImagesAsync(It.IsAny<Guid>(), images, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<HousingImage>());

            _housingRepository.Setup(r => r.AddAsync(It.IsAny<HousingEntity>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

            await _service.AddHousingAsync(ownerId, request, CancellationToken.None);

            _imageService.Verify(i => i.UploadImagesAsync(It.IsAny<Guid>(), images, It.IsAny<CancellationToken>()), Times.Once);
            _housingRepository.Verify(r => r.AddAsync(It.IsAny<HousingEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ThrowsArgumentException_WhenOwnerIdIsInvalid()
        {
            var request = BuildRequest();

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddHousingAsync("not-a-guid", request, CancellationToken.None));

            Assert.Equal("Некорректный формат ID.", ex.Message);
        }
    }
}
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Enums;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Exceptions;
using RentIt.Housing.Domain.Mappings.Housing;
using RentIt.Housing.Domain.Services.Interfaces;
using RentIt.MessageBroker.Contracts.Events;
using Serilog;

namespace RentIt.Housing.Tests.HousingService
{
    public class UpdateHousingAsyncTests
    {
        private readonly Mock<IHousingRepository> _housingRepository;
        private readonly Mock<IHousingImageService> _imageService;
        private readonly Mock<IValidator<UpdateHousingRequest>> _validator;
        private readonly Mock<IEventBus> _eventBus;
        private readonly Mock<ILogger> _logger;
        private readonly IMapper _mapper;

        private readonly Domain.Services.HousingService _service;

        public UpdateHousingAsyncTests()
        {
            _housingRepository = new Mock<IHousingRepository>();
            _imageService = new Mock<IHousingImageService>();
            _validator = new Mock<IValidator<UpdateHousingRequest>>();
            _eventBus = new Mock<IEventBus>();
            _logger = new Mock<ILogger>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UpdateHousingRequestProfile>();
            });
            _mapper = config.CreateMapper();

            _service = new Domain.Services.HousingService(
                _housingRepository.Object,
                _mapper,
                new Mock<IValidator<CreateHousingRequest>>().Object,
                new Mock<IValidator<GetFilteredHousingsRequest>>().Object,
                _validator.Object,
                _imageService.Object,
                new Mock<IUserIntegrationService>().Object,
                new Mock<IBookingIntegrationService>().Object,
                new Mock<ISpamProfanityFilterService>().Object,
                _logger.Object,
                _eventBus.Object
            );
        }

        private UpdateHousingRequest BuildRequest(
            decimal? pricePerNight = null,
            List<IFormFile>? addedImages = null,
            List<string>? removedImages = null)
        {
            return new UpdateHousingRequest(
                Title: "Updated Title",
                Description: "Updated Description",
                Country: "Updated Country",
                City: "Updated City",
                Address: "Updated Address",
                PricePerNight: pricePerNight,
                NumberOfRooms: 3,
                Amenities: ["WiFi", "Parking"],
                Status: HousingStatus.Available,
                EstimatedEndDate: DateTime.UtcNow.AddDays(30),
                AddedImages: addedImages,
                RemovedImages: removedImages
            );
        }

        [Fact]
        public async Task UpdatesHousingSuccessfully_WhenRequestIsValid_WithoutImages()
        {
            var housingId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var request = BuildRequest(pricePerNight: 150m);

            var existingHousing = new HousingEntity
            {
                HousingId = housingId,
                OwnerId = Guid.Parse(userId),
                PricePerNight = 100m,
                Images = []
            };

            _validator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _housingRepository.Setup(r => r.GetByIdAsync(housingId, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(existingHousing);

            _housingRepository.Setup(r => r.UpdateAsync(It.IsAny<HousingEntity>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

            _imageService.Setup(i => i.UpdateImagesAsync(housingId, null, null, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<HousingImage>());

            _eventBus.Setup(e => e.PublishAsync(It.IsAny<HousingUpdatedEvent>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

            await _service.UpdateHousingAsync(housingId, userId, request, CancellationToken.None);

            _housingRepository.Verify(r => r.UpdateAsync(It.Is<HousingEntity>(h =>
                h.HousingId == housingId &&
                h.Title == request.Title &&
                h.PricePerNight == request.PricePerNight &&
                h.UpdatedAt != default), CancellationToken.None), Times.Once());

            _eventBus.Verify(e => e.PublishAsync(It.Is<HousingUpdatedEvent>(ev =>
                ev.HousingId == housingId &&
                ev.NewPricePerNight == request.PricePerNight), CancellationToken.None), Times.Once());

            _imageService.Verify(i => i.UpdateImagesAsync(housingId, null, null, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdatesHousingSuccessfully_WhenRequestIsValid_WithImages()
        {
            var housingId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var addedImages = new List<IFormFile> { Mock.Of<IFormFile>() };
            var removedImages = new List<string> { "image1.jpg" };
            var request = BuildRequest(pricePerNight: 100m, addedImages: addedImages, removedImages: removedImages);
            var existingHousing = new HousingEntity
            {
                HousingId = housingId,
                OwnerId = Guid.Parse(userId),
                PricePerNight = 100m,
                Images = new List<HousingImage>()
            };

            _validator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _housingRepository.Setup(r => r.GetByIdAsync(housingId, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(existingHousing);

            _housingRepository.Setup(r => r.UpdateAsync(It.IsAny<HousingEntity>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

            _imageService.Setup(i => i.UpdateImagesAsync(housingId, addedImages, removedImages, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<HousingImage> { new() { ImageUrl = "new-image.jpg" } });

            await _service.UpdateHousingAsync(housingId, userId, request, CancellationToken.None);

            _housingRepository.Verify(r => r.UpdateAsync(It.Is<HousingEntity>(h =>
                h.HousingId == housingId &&
                h.Title == request.Title &&
                h.Images.Count == 1 &&
                h.UpdatedAt != default), CancellationToken.None), Times.Once);

            _imageService.Verify(i => i.UpdateImagesAsync(housingId, addedImages, removedImages, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task ThrowsNotFoundException_WhenHousingDoesNotExist()
        {
            var housingId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var request = BuildRequest();

            _validator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

            _housingRepository.Setup(r => r.GetByIdAsync(housingId, It.IsAny<CancellationToken>()))
                              .ReturnsAsync((HousingEntity?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UpdateHousingAsync(housingId, userId, request, CancellationToken.None));

            _housingRepository.Verify(r => r.UpdateAsync(It.IsAny<HousingEntity>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ThrowsUnauthorizedAccessException_WhenUserIsNotOwner()
        {
            var housingId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var request = BuildRequest();
            var existingHousing = new HousingEntity
            {
                HousingId = housingId,
                OwnerId = Guid.NewGuid(), 
                PricePerNight = 100m
            };

            _validator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

            _housingRepository.Setup(r => r.GetByIdAsync(housingId, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(existingHousing);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.UpdateHousingAsync(housingId, userId, request, CancellationToken.None));

            _housingRepository.Verify(r => r.UpdateAsync(It.IsAny<HousingEntity>(), It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}
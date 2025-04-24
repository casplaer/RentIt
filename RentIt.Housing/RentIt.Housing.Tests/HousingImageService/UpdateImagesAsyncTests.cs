using Microsoft.AspNetCore.Http;
using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Services.Interfaces;
using Serilog;

namespace RentIt.Housing.Tests.HousingImageService
{
    public class UpdateImagesAsyncTests
    {
        private readonly Mock<IHousingImageRepository> _imageRepository;
        private readonly Mock<IFileStorageService> _fileStorageService;
        private readonly Mock<ILogger> _logger;

        private readonly Domain.Services.HousingImageService _service;

        public UpdateImagesAsyncTests()
        {
            _imageRepository = new Mock<IHousingImageRepository>();
            _fileStorageService = new Mock<IFileStorageService>();
            _logger = new Mock<ILogger>();
            _service = new Domain.Services.HousingImageService(_imageRepository.Object, _fileStorageService.Object, _logger.Object);
        }

        [Fact]
        public async Task AddsNewImages_WhenAddedImagesAreProvided()
        {
            var housingId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;
            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(f => f.Length).Returns(100);
            var addedImages = new List<IFormFile> { formFileMock.Object };

            _imageRepository.Setup(r => r.GetImagesByHousingIdAsync(housingId, cancellationToken))
                .ReturnsAsync(new List<HousingImage>());

            _fileStorageService.Setup(s => s.SaveFileAsync(It.IsAny<IFormFile>(), cancellationToken))
                .ReturnsAsync("http://image.jpg");

            _imageRepository.Setup(r => r.AddAsync(It.IsAny<HousingImage>(), cancellationToken))
                .Returns(Task.CompletedTask);

            var result = await _service.UpdateImagesAsync(housingId, addedImages, null, cancellationToken);

            Assert.Single(result);
            Assert.Equal("http://image.jpg", result[0].ImageUrl);
        }

        [Fact]
        public async Task RemovesImages_WhenRemovedImagePathsAreProvided()
        {
            var housingId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;
            var image = new HousingImage
            {
                ImageId = Guid.NewGuid(),
                HousingId = housingId,
                ImageUrl = "http://localhost/images/abc.jpg",
                Order = 1
            };

            _imageRepository.Setup(r => r.GetImagesByHousingIdAsync(housingId, cancellationToken))
                .ReturnsAsync(new List<HousingImage> { image });

            _imageRepository.Setup(r => r.GetHousingImageByIdAsync(image.ImageId, cancellationToken))
                .ReturnsAsync(image);

            _fileStorageService.Setup(s => s.DeleteFile(It.IsAny<string>()))
                .Returns(true);

            _imageRepository.Setup(r => r.DeleteAsync(image.ImageId, cancellationToken))
                .Returns(Task.CompletedTask);

            var result = await _service.UpdateImagesAsync(housingId, null, ["images/abc.jpg"], cancellationToken);

            Assert.Empty(result);
        }

        [Fact]
        public async Task SkipsNonExistentImages_WhenPathNotFound()
        {
            var housingId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;

            _imageRepository.Setup(r => r.GetImagesByHousingIdAsync(housingId, cancellationToken))
                .ReturnsAsync(new List<HousingImage>());

            var result = await _service.UpdateImagesAsync(housingId, null, ["notfound.jpg"], cancellationToken);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ReturnsImages_WhenNoChanges()
        {
            var housingId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;
            var images = new List<HousingImage>
            {
                new() {
                    ImageId = Guid.NewGuid(),
                    HousingId = housingId,
                    ImageUrl = "http://img1.jpg",
                    Order = 1
                }
            };

            _imageRepository.Setup(r => r.GetImagesByHousingIdAsync(housingId, cancellationToken))
                .ReturnsAsync(images);

            var result = await _service.UpdateImagesAsync(housingId, null, null, cancellationToken);

            Assert.Single(result);
            Assert.Equal("http://img1.jpg", result[0].ImageUrl);
        }
    }
}
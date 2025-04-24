using Microsoft.AspNetCore.Http;
using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Services.Interfaces;
using Serilog;

namespace RentIt.Housing.Tests.HousingImageService
{
    public class UploadImagesAsyncTests
    {
        private readonly Mock<IHousingImageRepository> _imageRepository;
        private readonly Mock<IFileStorageService> _fileStorageService;
        private readonly Mock<ILogger> _logger;
        private readonly Domain.Services.HousingImageService _service;

        public UploadImagesAsyncTests()
        {
            _imageRepository = new Mock<IHousingImageRepository>();
            _fileStorageService = new Mock<IFileStorageService>();
            _logger = new Mock<ILogger>();

            _service = new Domain.Services.HousingImageService(
                _imageRepository.Object,
                _fileStorageService.Object,
                _logger.Object
            );
        }

        private IFormFile CreateFormFile(string name = "file.jpg", long length = 100)
        {
            var mock = new Mock<IFormFile>();
            mock.Setup(f => f.FileName).Returns(name);
            mock.Setup(f => f.Length).Returns(length);
            return mock.Object;
        }

        [Fact]
        public async Task UploadImagesAsync_ReturnsUploadedImages_WhenValidImagesProvided()
        {
            var housingId = Guid.NewGuid();
            var images = new List<IFormFile> { CreateFormFile(), CreateFormFile("file2.jpg") };

            _fileStorageService.Setup(f => f.ValidateImageFile(It.IsAny<IFormFile>()));
            _fileStorageService.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync("http://image.url");

            _imageRepository.Setup(r => r.AddAsync(It.IsAny<HousingImage>(), It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var result = await _service.UploadImagesAsync(housingId, images, CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.All(result, img => Assert.Equal(housingId, img.HousingId));
        }

        [Fact]
        public async Task UploadImagesAsync_ThrowsArgumentException_WhenImagesIsNull()
        {
            var housingId = Guid.NewGuid();

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UploadImagesAsync(housingId, null!, CancellationToken.None));

            Assert.Equal("images", ex.ParamName);
        }

        [Fact]
        public async Task UploadImagesAsync_ThrowsArgumentException_WhenImagesIsEmpty()
        {
            var housingId = Guid.NewGuid();

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UploadImagesAsync(housingId, Enumerable.Empty<IFormFile>(), CancellationToken.None));

            Assert.Equal("images", ex.ParamName);
        }

        [Fact]
        public async Task UploadImagesAsync_IgnoresImagesWithZeroLength()
        {
            var housingId = Guid.NewGuid();
            var images = new List<IFormFile> { CreateFormFile(length: 0), CreateFormFile("valid.jpg") };

            _fileStorageService.Setup(f => f.ValidateImageFile(It.IsAny<IFormFile>()));
            _fileStorageService.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync("http://image.url");

            _imageRepository.Setup(r => r.AddAsync(It.IsAny<HousingImage>(), It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var result = await _service.UploadImagesAsync(housingId, images, CancellationToken.None);

            Assert.Single(result);
        }

        [Fact]
        public async Task UploadImagesAsync_CallsDependencies_CorrectNumberOfTimes()
        {
            var housingId = Guid.NewGuid();
            var images = new List<IFormFile> { CreateFormFile(), CreateFormFile() };

            _fileStorageService.Setup(f => f.ValidateImageFile(It.IsAny<IFormFile>()));
            _fileStorageService.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync("http://image.url");

            _imageRepository.Setup(r => r.AddAsync(It.IsAny<HousingImage>(), It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            await _service.UploadImagesAsync(housingId, images, CancellationToken.None);

            _fileStorageService.Verify(f => f.ValidateImageFile(It.IsAny<IFormFile>()), Times.Exactly(2));
            _fileStorageService.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _imageRepository.Verify(r => r.AddAsync(It.IsAny<HousingImage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }

}

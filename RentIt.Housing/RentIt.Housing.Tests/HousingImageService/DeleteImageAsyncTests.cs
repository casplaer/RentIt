using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Services.Interfaces;
using Serilog;

namespace RentIt.Housing.Tests.HousingImageService
{
    public class DeleteImageAsyncTests
    {
        private readonly Mock<IHousingImageRepository> _imageRepository;
        private readonly Mock<IFileStorageService> _fileStorageService;
        private readonly Mock<ILogger> _logger;

        private readonly Domain.Services.HousingImageService _service;

        public DeleteImageAsyncTests()
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

        [Fact]
        public async Task DeletesImageSuccessfully_WhenImageExists()
        {
            var imageId = Guid.NewGuid();

            var imageEntity = new HousingImage
            {
                ImageId = imageId,
                ImageUrl = "images/test.jpg"
            };

            _imageRepository.Setup(r => r.GetHousingImageByIdAsync(imageId, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(imageEntity);

            _fileStorageService.Setup(f => f.DeleteFile(It.IsAny<string>())).Returns(true);

            _imageRepository.Setup(r => r.DeleteAsync(imageId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await _service.DeleteImageAsync(imageId, CancellationToken.None);

            _imageRepository.Verify(r => r.DeleteAsync(imageId, It.IsAny<CancellationToken>()), Times.Once);

            _fileStorageService.Verify(f => f.DeleteFile("test.jpg"), Times.Once);

            _logger.Verify(l => l.Information("Файл изображения удален с диска: {FilePath}", "test.jpg"), Times.Once);
        }

        [Fact]
        public async Task ThrowsArgumentException_WhenImageDoesNotExist()
        {
            var imageId = Guid.NewGuid();

            _imageRepository.Setup(r => r.GetHousingImageByIdAsync(imageId, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((HousingImage)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteImageAsync(imageId, CancellationToken.None));
        }
    }
}
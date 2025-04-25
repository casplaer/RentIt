using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Services.Interfaces;
using Serilog;

namespace RentIt.Housing.Tests.HousingImageService
{
    public class ClearImagesAsyncTests
    {
        private readonly Mock<IHousingImageRepository> _imageRepository;
        private readonly Mock<IFileStorageService> _fileStorageService;
        private readonly Mock<ILogger> _logger;

        private readonly Domain.Services.HousingImageService _service;

        public ClearImagesAsyncTests()
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
        public async Task ClearsImagesSuccessfully_WhenImagesExist()
        {
            var images = new List<HousingImage>
            {
                new() 
                { 
                    ImageId = Guid.NewGuid(), 
                    ImageUrl = "images/test1.jpg" 
                },
                new()
                { 
                    ImageId = Guid.NewGuid(), 
                    ImageUrl = "images/test2.jpg" 
                }
            };

            _imageRepository.Setup(r => r.GetHousingImageByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new HousingImage { ImageId = Guid.NewGuid() });

            _fileStorageService.Setup(f => f.DeleteFile(It.IsAny<string>())).Returns(true);

            _imageRepository.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await _service.ClearImagesAsync(images, CancellationToken.None);

            _imageRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(images.Count));
            _fileStorageService.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Exactly(images.Count));
        }

        [Fact]
        public async Task DoesNotThrowException_WhenImagesListIsEmpty()
        {
            var images = new List<HousingImage>();

            await _service.ClearImagesAsync(images, CancellationToken.None);

            _imageRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _fileStorageService.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Never);

            _logger.Verify(l => l.Information(It.IsAny<string>()), Times.Never);
        }
    }
}
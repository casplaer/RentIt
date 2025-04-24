using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Services.Interfaces;
using Serilog;

namespace RentIt.Housing.Tests.HousingImageService
{
    public class GetImagesByHousingIdAsyncTests
    {
        private readonly Mock<IHousingImageRepository> _imageRepository;
        private readonly Mock<IFileStorageService> _fileStorageService;
        private readonly Mock<ILogger> _logger;

        private readonly Domain.Services.HousingImageService _service;

        public GetImagesByHousingIdAsyncTests()
        {
            _imageRepository = new Mock<IHousingImageRepository>();
            _fileStorageService = new Mock<IFileStorageService>();
            _logger = new Mock<ILogger>();

            _service = new Domain.Services.HousingImageService(
                _imageRepository.Object,
                _fileStorageService.Object,
                _logger.Object);
        }

        [Fact]
        public async Task ReturnsImages_WhenImagesExist()
        {
            var housingId = Guid.NewGuid();
            var expectedImages = new List<HousingImage>
            {
                new()
                {
                    ImageId = Guid.NewGuid(),
                    ImageUrl = "http://image1.com" 
                },
                new()
                { 
                    ImageId = Guid.NewGuid(), 
                    ImageUrl = "http://image2.com" 
                }
            };

            _imageRepository.Setup(r => r.GetImagesByHousingIdAsync(housingId, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expectedImages);

            var result = await _service.GetImagesByHousingIdAsync(housingId, CancellationToken.None);

            Assert.Equal(expectedImages, result);

            _imageRepository.Verify(r => r.GetImagesByHousingIdAsync(housingId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReturnsEmptyList_WhenNoImagesExist()
        {
            var housingId = Guid.NewGuid();

            _imageRepository.Setup(r => r.GetImagesByHousingIdAsync(housingId, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<HousingImage>());

            var result = await _service.GetImagesByHousingIdAsync(housingId, CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ThrowsException_WhenRepositoryThrows()
        {
            var housingId = Guid.NewGuid();

            _imageRepository.Setup(r => r.GetImagesByHousingIdAsync(housingId, It.IsAny<CancellationToken>()))
                            .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _service.GetImagesByHousingIdAsync(housingId, CancellationToken.None));
        }
    }
}
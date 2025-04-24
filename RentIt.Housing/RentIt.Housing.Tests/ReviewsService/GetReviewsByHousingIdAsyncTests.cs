using AutoMapper;
using FluentValidation;
using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Contracts.Requests.Reviews;
using RentIt.Housing.Domain.Services.Interfaces;
using Serilog;

namespace RentIt.Housing.Tests.ReviewsService
{
    public class GetReviewsByHousingIdAsyncTests
    {
        private readonly Mock<IReviewRepository> _reviewRepository;
        private readonly Mock<IUserIntegrationService> _userIntegrationService;
        private readonly Mock<IHousingService> _housingService;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IValidator<CreateReviewRequest>> _createReviewRequestValidator;
        private readonly Mock<IValidator<UpdateReviewRequest>> _updateReviewRequestValidator;
        private readonly Mock<ILogger> _logger;
        private readonly Domain.Services.ReviewsService _service;

        public GetReviewsByHousingIdAsyncTests()
        {
            _reviewRepository = new Mock<IReviewRepository>();
            _userIntegrationService = new Mock<IUserIntegrationService>();
            _housingService = new Mock<IHousingService>();
            _mapper = new Mock<IMapper>();
            _createReviewRequestValidator = new Mock<IValidator<CreateReviewRequest>>();
            _updateReviewRequestValidator = new Mock<IValidator<UpdateReviewRequest>>();
            _logger = new Mock<ILogger>();
            _service = new Domain.Services.ReviewsService(
                _reviewRepository.Object,
                _userIntegrationService.Object,
                _housingService.Object,
                _mapper.Object,
                _createReviewRequestValidator.Object,
                _updateReviewRequestValidator.Object,
                _logger.Object
            );
        }

        [Fact]
        public async Task GetReviewsByHousingIdAsync_ReturnsReviews_WhenReviewsExist()
        {
            var housingId = Guid.NewGuid();
            var reviews = new List<Review>
            {
                new Review { ReviewId = Guid.NewGuid(), HousingId = housingId, Comment = "Great place!" },
                new Review { ReviewId = Guid.NewGuid(), HousingId = housingId, Comment = "Not bad." }
            };

            _reviewRepository.Setup(r => r.GetReviewsByHousingIdAsync(housingId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(reviews);

            var result = await _service.GetReviewsByHousingIdAsync(housingId, CancellationToken.None);

            Assert.Equal(reviews.Count, result.Count());
            Assert.Contains(result, r => r.Comment == "Great place!");
            Assert.Contains(result, r => r.Comment == "Not bad.");
        }

        [Fact]
        public async Task GetReviewsByHousingIdAsync_ReturnsEmptyList_WhenNoReviewsExist()
        {
            var housingId = Guid.NewGuid();
            var reviews = new List<Review>();

            _reviewRepository.Setup(r => r.GetReviewsByHousingIdAsync(housingId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(reviews);

            var result = await _service.GetReviewsByHousingIdAsync(housingId, CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetReviewsByHousingIdAsync_ThrowsException_WhenHousingNotFound()
        {
            var housingId = Guid.NewGuid();
            _reviewRepository.Setup(r => r.GetReviewsByHousingIdAsync(housingId, It.IsAny<CancellationToken>()))
                             .ThrowsAsync(new Exception("Housing not found"));

            await Assert.ThrowsAsync<Exception>(() => _service.GetReviewsByHousingIdAsync(housingId, CancellationToken.None));
        }
    }
}
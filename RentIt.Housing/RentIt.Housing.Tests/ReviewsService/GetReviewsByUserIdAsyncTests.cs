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
    public class GetReviewsByUserIdAsyncTests
    {
        private readonly Mock<IReviewRepository> _reviewRepository;
        private readonly Mock<IUserIntegrationService> _userIntegrationService;
        private readonly Mock<IHousingService> _housingService;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IValidator<CreateReviewRequest>> _createReviewRequestValidator;
        private readonly Mock<IValidator<UpdateReviewRequest>> _updateReviewRequestValidator;
        private readonly Mock<ILogger> _logger;
        private readonly Domain.Services.ReviewsService _service;

        public GetReviewsByUserIdAsyncTests()
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
        public async Task GetReviewsByUserIdAsync_ReturnsReviews_WhenReviewsExist()
        {
            var userId = Guid.NewGuid();
            var reviews = new List<Review>
            {
                new Review { ReviewId = Guid.NewGuid(), UserId = userId, Comment = "Excellent service!" },
                new Review { ReviewId = Guid.NewGuid(), UserId = userId, Comment = "Not bad at all." }
            };

            _reviewRepository.Setup(r => r.GetAllReviewsAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(reviews);

            var result = await _service.GetReviewsByUserIdAsync(userId, CancellationToken.None);

            Assert.Equal(reviews.Count, result.Count());
            Assert.Contains(result, r => r.Comment == "Excellent service!");
            Assert.Contains(result, r => r.Comment == "Not bad at all.");
        }

        [Fact]
        public async Task GetReviewsByUserIdAsync_ReturnsEmptyList_WhenNoReviewsExist()
        {
            var userId = Guid.NewGuid();
            var reviews = new List<Review>(); 

            _reviewRepository.Setup(r => r.GetAllReviewsAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(reviews);

            var result = await _service.GetReviewsByUserIdAsync(userId, CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetReviewsByUserIdAsync_ThrowsException_WhenUserIdNotFound()
        {
            var userId = Guid.NewGuid();
            _reviewRepository.Setup(r => r.GetAllReviewsAsync(It.IsAny<CancellationToken>()))
                             .ThrowsAsync(new Exception("User reviews not found"));

            await Assert.ThrowsAsync<Exception>(() => _service.GetReviewsByUserIdAsync(userId, CancellationToken.None));
        }
    }
}
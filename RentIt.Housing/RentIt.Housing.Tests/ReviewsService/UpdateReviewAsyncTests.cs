using AutoMapper;
using FluentValidation;
using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Contracts.Requests.Reviews;
using RentIt.Housing.Domain.Contracts.Responses.Housing;
using RentIt.Housing.Domain.Exceptions;
using RentIt.Housing.Domain.Services.Interfaces;
using Serilog;

namespace RentIt.Housing.Tests.ReviewsService
{
    public class UpdateReviewAsyncTests
    {
        private readonly Mock<IReviewRepository> _reviewRepositoryMock;
        private readonly Mock<IUserIntegrationService> _userIntegrationServiceMock;
        private readonly Mock<IHousingService> _housingServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IValidator<UpdateReviewRequest>> _updateReviewRequestValidatorMock;
        private readonly Mock<IValidator<CreateReviewRequest>> _createReviewRequestValidatorMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Domain.Services.ReviewsService _reviewsService;

        public UpdateReviewAsyncTests()
        {
            _reviewRepositoryMock = new Mock<IReviewRepository>();
            _userIntegrationServiceMock = new Mock<IUserIntegrationService>();
            _housingServiceMock = new Mock<IHousingService>();
            _mapperMock = new Mock<IMapper>();
            _updateReviewRequestValidatorMock = new Mock<IValidator<UpdateReviewRequest>>();
            _createReviewRequestValidatorMock = new Mock<IValidator<CreateReviewRequest>>();
            _loggerMock = new Mock<ILogger>();

            _reviewsService = new Domain.Services.ReviewsService(
                _reviewRepositoryMock.Object,
                _userIntegrationServiceMock.Object,
                _housingServiceMock.Object,
                _mapperMock.Object,
                _createReviewRequestValidatorMock.Object,
                _updateReviewRequestValidatorMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task UpdateReviewAsync_ReviewNotFound_ThrowsNotFoundException()
        {
            var reviewId = Guid.NewGuid();
            var userId = "user123";
            var updateRequest = new UpdateReviewRequest(5, "Great!");
            var cancellationToken = CancellationToken.None;

            _reviewRepositoryMock
                .Setup(r => r.GetReviewByIdAsync(reviewId, cancellationToken))
                .ReturnsAsync((Review)null);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _reviewsService.UpdateReviewAsync(reviewId, userId, updateRequest, cancellationToken));
            Assert.Equal("Отзыва с таким ID не найдено.", exception.Message);
        }

        [Fact]
        public async Task UpdateReviewAsync_HousingNotFound_ThrowsNotFoundException()
        {
            var reviewId = Guid.NewGuid();
            var userId = "user123"; 
            var updateRequest = new UpdateReviewRequest(5, "Great!");
            var cancellationToken = CancellationToken.None;
            var review = new Review { ReviewId = reviewId, HousingId = Guid.NewGuid() };

            _reviewRepositoryMock
                .Setup(r => r.GetReviewByIdAsync(reviewId, cancellationToken))
                .ReturnsAsync(review);

            _housingServiceMock
                .Setup(h => h.GetByIdAsync(It.IsAny<Guid>(), cancellationToken))
                .ReturnsAsync((GetHousingByIdResponse)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _reviewsService.UpdateReviewAsync(reviewId, userId, updateRequest, cancellationToken));

            Assert.Equal("Некорректный формат ID комментатора.", exception.Message);
        }

        [Fact]
        public async Task UpdateReviewAsync_ReviewNotFoundInHousing_ThrowsNotFoundException()
        {
            var reviewId = Guid.NewGuid();
            var userId = "user123"; 
            var updateRequest = new UpdateReviewRequest(5, "Great!");
            var cancellationToken = CancellationToken.None;
            var review = new Review { ReviewId = reviewId, HousingId = Guid.NewGuid() };
            var housing = new HousingEntity
            {
                HousingId = review.HousingId,
                Reviews = new List<Review>()
            };

            _reviewRepositoryMock
                .Setup(r => r.GetReviewByIdAsync(reviewId, cancellationToken))
                .ReturnsAsync(review);

            _housingServiceMock
                .Setup(h => h.GetByIdAsync(It.IsAny<Guid>(), cancellationToken))
                .ReturnsAsync(new GetHousingByIdResponse(housing, null));

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _reviewsService.UpdateReviewAsync(reviewId, userId, updateRequest, cancellationToken));

            Assert.Equal("Некорректный формат ID комментатора.", exception.Message);
        }

        [Fact]
        public async Task UpdateReviewAsync_SuccessfullyUpdatesReview()
        {
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var updateRequest = new UpdateReviewRequest(5, "Updated review!");
            var cancellationToken = CancellationToken.None;
            var review = new Review { ReviewId = reviewId, HousingId = Guid.NewGuid(), UserId = userId };
            var housing = new HousingEntity
            {
                HousingId = review.HousingId,
                Reviews = new List<Review> { review } 
            };

            _reviewRepositoryMock
                .Setup(r => r.GetReviewByIdAsync(reviewId, cancellationToken))
                .ReturnsAsync(review);

            _housingServiceMock
                .Setup(h => h.GetByIdAsync(It.IsAny<Guid>(), cancellationToken))
                .ReturnsAsync(new GetHousingByIdResponse(housing, null));

            _mapperMock
                .Setup(m => m.Map(It.IsAny<UpdateReviewRequest>(), It.IsAny<Review>()))
                .Returns(review);

            _reviewRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Review>(), cancellationToken))
                .Returns(Task.CompletedTask);

            _housingServiceMock
                .Setup(h => h.UpdateHousingAsync(It.IsAny<HousingEntity>(), cancellationToken))
                .Returns(Task.CompletedTask);

            await _reviewsService.UpdateReviewAsync(reviewId, userId.ToString(), updateRequest, cancellationToken);

            _reviewRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Review>(), cancellationToken), Times.Once);
            _housingServiceMock.Verify(h => h.UpdateHousingAsync(It.IsAny<HousingEntity>(), cancellationToken), Times.Once);
        }
    }
}
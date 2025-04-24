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
    public class DeleteReviewAsyncTests
    {
        private readonly Mock<IReviewRepository> _reviewRepositoryMock;
        private readonly Mock<IHousingService> _housingServiceMock;

        private readonly Domain.Services.ReviewsService _reviewsService;

        public DeleteReviewAsyncTests()
        {
            _reviewRepositoryMock = new Mock<IReviewRepository>();
            _housingServiceMock = new Mock<IHousingService>();
            var loggerMock = new Mock<ILogger>();

            _reviewsService = new Domain.Services.ReviewsService(
                _reviewRepositoryMock.Object,
                Mock.Of<IUserIntegrationService>(),
                _housingServiceMock.Object,
                Mock.Of<IMapper>(),
                Mock.Of<IValidator<CreateReviewRequest>>(),
                Mock.Of<IValidator<UpdateReviewRequest>>(),
                loggerMock.Object
            );
        }

        [Fact]
        public async Task DeleteReviewAsync_ReviewNotFound_ThrowsNotFoundException()
        {
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var cancellationToken = CancellationToken.None;

            _reviewRepositoryMock
                .Setup(r => r.GetReviewByIdAsync(reviewId, cancellationToken))
                .ReturnsAsync((Review)null); 

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _reviewsService.DeleteReviewAsync(reviewId, userId, cancellationToken));

            Assert.Equal("Отзыва с таким ID не найдено.", exception.Message);
        }

        [Fact]
        public async Task DeleteReviewAsync_HousingNotFound_ThrowsNotFoundException()
        {
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var cancellationToken = CancellationToken.None;
            
            var review = new Review
            { 
                ReviewId = reviewId, 
                HousingId = Guid.NewGuid() 
            };

            _reviewRepositoryMock
                .Setup(r => r.GetReviewByIdAsync(reviewId, cancellationToken))
                .ReturnsAsync(review);

            _housingServiceMock
                .Setup(h => h.GetByIdAsync(It.IsAny<Guid>(), cancellationToken))
                .ReturnsAsync((GetHousingByIdResponse)null); 

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _reviewsService.DeleteReviewAsync(reviewId, userId, cancellationToken));

            Assert.Equal("Попытка неавторизованного доступа к комментарию.", exception.Message);
        }

        [Fact]
        public async Task DeleteReviewAsync_SuccessfullyDeletesReview()
        {
            var reviewId = Guid.NewGuid();
            var housingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;

            var review = new Review
            {
                ReviewId = reviewId,
                HousingId = housingId,
                UserId = userId
            };

            var housing = new HousingEntity
            {
                HousingId = housingId,
                Reviews = new List<Review> { review },
                Rating = 5.0
            };

            _reviewRepositoryMock
                .Setup(r => r.GetReviewByIdAsync(reviewId, cancellationToken))
                .ReturnsAsync(review);

            _housingServiceMock
                .Setup(h => h.GetByIdAsync(housingId, cancellationToken))
                .ReturnsAsync(new GetHousingByIdResponse(housing, null));

            _reviewRepositoryMock
                .Setup(r => r.DeleteAsync(reviewId, cancellationToken))
                .Returns(Task.CompletedTask);

            _housingServiceMock
                .Setup(h => h.UpdateHousingAsync(housing, cancellationToken))
                .Returns(Task.CompletedTask);

            await _reviewsService.DeleteReviewAsync(reviewId, userId.ToString(), cancellationToken);

            _reviewRepositoryMock.Verify(r => r.DeleteAsync(reviewId, cancellationToken), Times.Once);
            _housingServiceMock.Verify(h => h.UpdateHousingAsync(housing, cancellationToken), Times.Once);

            Assert.Empty(housing.Reviews);
            Assert.Equal(0, housing.Rating);
        }


        [Fact]
        public async Task DeleteReviewAsync_UnauthorizedAccess_ThrowsUnauthorizedAccessException()
        {
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var cancellationToken = CancellationToken.None;
            var review = new Review { ReviewId = reviewId, HousingId = Guid.NewGuid(), UserId = Guid.NewGuid() };

            _reviewRepositoryMock
                .Setup(r => r.GetReviewByIdAsync(reviewId, cancellationToken))
                .ReturnsAsync(review);

            _housingServiceMock
                .Setup(h => h.GetByIdAsync(It.IsAny<Guid>(), cancellationToken))
                .ReturnsAsync(new GetHousingByIdResponse(new HousingEntity(), null));

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _reviewsService.DeleteReviewAsync(reviewId, userId, cancellationToken));

            Assert.Equal("Попытка неавторизованного доступа к комментарию.", exception.Message);
        }
    }
}
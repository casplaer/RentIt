using AutoMapper;
using FluentValidation;
using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Enums;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Contracts.Dto.Users;
using RentIt.Housing.Domain.Contracts.Requests.Reviews;
using RentIt.Housing.Domain.Contracts.Responses.Housing;
using RentIt.Housing.Domain.Exceptions;
using RentIt.Housing.Domain.Mappings.Reviews;
using RentIt.Housing.Domain.Services.Interfaces;
using Serilog;

namespace RentIt.Housing.Tests.ReviewsService
{
    public class AddReviewAsyncTests
    {
        private readonly string _userId;
        private readonly Guid _housingId;
        private readonly CreateReviewRequest _request;
        private readonly CancellationToken _cancellationToken;
        private readonly Mock<IHousingService> _housingServiceMock;
        private readonly Mock<IReviewRepository> _reviewRepositoryMock;
        private readonly IMapper _mapper;
        private readonly Mock<IValidator<CreateReviewRequest>> _validatorMock;

        private readonly Domain.Services.ReviewsService _reviewService;

        public AddReviewAsyncTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CreateReviewRequestProfile>();
            });
            _mapper = config.CreateMapper();

            _userId = Guid.NewGuid().ToString();
            _housingId = Guid.NewGuid();
            _request = new CreateReviewRequest(5, "Great place!");
            _cancellationToken = CancellationToken.None;

            _housingServiceMock = new Mock<IHousingService>();
            _reviewRepositoryMock = new Mock<IReviewRepository>();
            _validatorMock = new Mock<IValidator<CreateReviewRequest>>();

            _reviewService = new Domain.Services.ReviewsService(
                _reviewRepositoryMock.Object,
                Mock.Of<IUserIntegrationService>(),
                _housingServiceMock.Object,
                _mapper,
                _validatorMock.Object,
                Mock.Of<IValidator<UpdateReviewRequest>>(),
                Mock.Of<ILogger>()
            );
        }

        [Fact]
        public async Task ThrowsArgumentException_WhenUserIdIsInvalid()
        {
            var invalidUserId = "invalid-id";

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _reviewService.AddReviewAsync(invalidUserId, _housingId, _request, _cancellationToken)
            );
        }

        [Fact]
        public async Task ThrowsNotFoundException_WhenHousingDoesNotExist()
        {
            _housingServiceMock
                .Setup(h => h.GetByIdAsync(_housingId, _cancellationToken))
                .ReturnsAsync((GetHousingByIdResponse?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _reviewService.AddReviewAsync(_userId, _housingId, _request, _cancellationToken)
            );
        }

        [Fact]
        public async Task ThrowsArgumentException_WhenHousingStatusIsUnpublished()
        {
            var housingEntity = new HousingEntity
            {
                HousingId = _housingId,
                Status = HousingStatus.Unpublished,
                Reviews = new List<Review>(),
            };

            _housingServiceMock
                .Setup(h => h.GetByIdAsync(_housingId, _cancellationToken))
                .ReturnsAsync(new GetHousingByIdResponse(
                    housingEntity,
                    new UserInfoDto("John", "Doe", "john.doe@example.com", "+1234567890")
                ));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _reviewService.AddReviewAsync(_userId, _housingId, _request, _cancellationToken)
            );
        }

        [Fact]
        public async Task AddsReviewSuccessfully_WhenAllConditionsMet()
        {
            var housingEntity = new HousingEntity
            {
                HousingId = _housingId,
                Status = HousingStatus.Available,
                Reviews = [],
            };

            _housingServiceMock
                .Setup(h => h.GetByIdAsync(_housingId, _cancellationToken))
                .ReturnsAsync(new GetHousingByIdResponse(
                    housingEntity,
                    new UserInfoDto("John", "Doe", "john.doe@example.com", "+1234567890")
                ));

            await _reviewService.AddReviewAsync(_userId, _housingId, _request, _cancellationToken);

            _reviewRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Review>(), _cancellationToken), Times.Once);
            _housingServiceMock.Verify(h => h.UpdateHousingAsync(It.IsAny<HousingEntity>(), _cancellationToken), Times.Once);
        }
    }
}
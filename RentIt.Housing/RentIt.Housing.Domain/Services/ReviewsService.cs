using AutoMapper;
using FluentValidation;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Enums;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Contracts.Requests.Reviews;
using RentIt.Housing.Domain.Exceptions;

namespace RentIt.Housing.Domain.Services
{
    public class ReviewsService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly UserIntegrationService _userIntegrationService;
        private readonly HousingService _housingService;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateReviewRequest> _createReviewRequestValidator;
        private readonly IValidator<UpdateReviewRequest> _updateReviewRequestValidator;

        public ReviewsService(
            IReviewRepository reviewRepository,
            UserIntegrationService userIntegrationService,
            HousingService housingService,
            IMapper mapper,
            IValidator<CreateReviewRequest> createReviewRequestValidator,
            IValidator<UpdateReviewRequest> updateReviewRequestValidator)
        {
            _reviewRepository = reviewRepository;
            _userIntegrationService = userIntegrationService;
            _housingService = housingService;
            _mapper = mapper;
            _createReviewRequestValidator = createReviewRequestValidator;
            _updateReviewRequestValidator = updateReviewRequestValidator;
        }

        public async Task<IEnumerable<Review>> GetReviewsByHousingIdAsync(
            Guid housingId, 
            CancellationToken cancellationToken)
        {
            return await _reviewRepository.GetReviewsByHousingIdAsync(housingId, cancellationToken);
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var reviews = (await _reviewRepository.GetAllReviewsAsync(cancellationToken)).Where(r => r.UserId == userId);

            return reviews;
        }

        public async Task AddReviewAsync(
            string userId,
            Guid housingId,
            CreateReviewRequest request,
            CancellationToken cancellationToken)
        {
            await _createReviewRequestValidator.ValidateAndThrowAsync(request, cancellationToken);

            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Некорректный формат ID.");
            }

            var housing = (await _housingService.GetByIdAsync(housingId, cancellationToken)).Housing;

            if( housing == null )
            {
                throw new NotFoundException("Собственности с таким ID не найдено.");
            }

            if( housing.Status == HousingStatus.Unpublished || housing.Status == HousingStatus.Rejected )
            {
                throw new ArgumentException("Нельзя добавить отзыв к неопубликованной собвстенности.");
            }    

            var review = _mapper.Map<Review>(request, opt =>
            {
                opt.Items["housingId"] = housingId;
                opt.Items["userId"] = userGuid;
            });

            housing.Reviews.Add(review);
            housing.Rating = housing.Reviews.Average(r => r.Rating);
            housing.UpdatedAt = DateTime.UtcNow;

            await _housingService.UpdateHousingAsync(housing, cancellationToken);

            await _reviewRepository.AddAsync(review, cancellationToken);
        }

        public async Task UpdateReviewAsync(
            Guid reviewId,
            UpdateReviewRequest request, 
            CancellationToken cancellationToken)
        {
            await _updateReviewRequestValidator.ValidateAndThrowAsync(request, cancellationToken);

            var reviewToUpdate = await _reviewRepository.GetReviewByIdAsync(reviewId, cancellationToken);

            if (reviewToUpdate == null)
            {
                throw new NotFoundException("Отзыва с таким ID не найдено.");
            }

            var housing = (await _housingService.GetByIdAsync(reviewToUpdate.HousingId, cancellationToken)).Housing;

            if (housing == null)
            {
                throw new NotFoundException($"Собственности с таким ID не найдено.");
            }

            _mapper.Map(request, reviewToUpdate);

            var existingReview = housing.Reviews.FirstOrDefault(r => r.ReviewId == reviewId);

            if (existingReview == null)
            {
                throw new NotFoundException("Отзыв не найден в коллекции этого жилья.");
            }

            _mapper.Map(request, existingReview);

            housing.Rating = housing.Reviews.Average(r => r.Rating);

            await _reviewRepository.UpdateAsync(reviewToUpdate, cancellationToken);

            await _housingService.UpdateHousingAsync(housing, cancellationToken);
        }

        public async Task DeleteReviewAsync(
            Guid reviewId,
            CancellationToken cancellationToken)
        {
            var reviewToDelete = await _reviewRepository.GetReviewByIdAsync(reviewId, cancellationToken);

            var housing = (await _housingService.GetByIdAsync(reviewToDelete.HousingId, cancellationToken)).Housing;

            if (housing == null)
            {
                throw new NotFoundException($"Собственности с таким ID не найдено.");
            }

            housing.Reviews.RemoveAll(r => r.ReviewId == reviewToDelete.ReviewId);

            housing.Rating = housing.Reviews.Any() ? housing.Reviews.Average(r => r.Rating) : 0;

            await _reviewRepository.DeleteAsync(reviewId, cancellationToken);

            await _housingService.UpdateHousingAsync(housing, cancellationToken);
        }
    }
}

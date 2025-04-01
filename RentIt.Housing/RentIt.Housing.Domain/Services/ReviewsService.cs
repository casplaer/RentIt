using AutoMapper;
using FluentValidation;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Enums;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Contracts.Requests.Reviews;
using RentIt.Housing.Domain.Exceptions;
using Serilog;

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
        private readonly ILogger _logger;

        public ReviewsService(
            IReviewRepository reviewRepository,
            UserIntegrationService userIntegrationService,
            HousingService housingService,
            IMapper mapper,
            IValidator<CreateReviewRequest> createReviewRequestValidator,
            IValidator<UpdateReviewRequest> updateReviewRequestValidator,
            ILogger logger)
        {
            _reviewRepository = reviewRepository;
            _userIntegrationService = userIntegrationService;
            _housingService = housingService;
            _mapper = mapper;
            _createReviewRequestValidator = createReviewRequestValidator;
            _updateReviewRequestValidator = updateReviewRequestValidator;
            _logger = logger;
        }

        public async Task<IEnumerable<Review>> GetReviewsByHousingIdAsync(
            Guid housingId,
            CancellationToken cancellationToken)
        {
            _logger.Information("Получение отзывов для собственности с ID {HousingId}", housingId);

            var reviews = await _reviewRepository.GetReviewsByHousingIdAsync(housingId, cancellationToken);

            _logger.Information("Найдено {Count} отзывов для собственности с ID {HousingId}", reviews.Count(), housingId);

            return reviews;
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            _logger.Information("Получение отзывов для пользователя с ID {UserId}", userId);

            var reviews = (await _reviewRepository.GetAllReviewsAsync(cancellationToken)).Where(r => r.UserId == userId);

            _logger.Information("Найдено {Count} отзывов для пользователя с ID {UserId}", reviews.Count(), userId);

            return reviews;
        }

        public async Task AddReviewAsync(
            string userId,
            Guid housingId,
            CreateReviewRequest request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Попытка добавления отзыва для собственности с ID {HousingId} от пользователя {UserId}", housingId, userId);

            await _createReviewRequestValidator.ValidateAndThrowAsync(request, cancellationToken);

            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.Error("Некорректный формат ID пользователя: {UserId}", userId);

                throw new ArgumentException("Некорректный формат ID.");
            }

            var housingResponse = await _housingService.GetByIdAsync(housingId, cancellationToken);
            var housing = housingResponse?.Housing;

            if (housing == null)
            {
                _logger.Warning("Собственность с ID {HousingId} не найдена", housingId);

                throw new NotFoundException("Собственности с таким ID не найдено.");
            }

            if (housing.Status == HousingStatus.Unpublished || housing.Status == HousingStatus.Rejected)
            {
                _logger.Warning("Невозможно добавить отзыв для собственности с ID {HousingId}, так как статус {Status}", housingId, housing.Status);

                throw new ArgumentException("Нельзя добавить отзыв к неопубликованной собственности.");
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

            _logger.Information("Отзыв с ID {ReviewId} успешно добавлен для собственности с ID {HousingId}", review.ReviewId, housingId);
        }

        public async Task UpdateReviewAsync(
            Guid reviewId,
            string userId,
            UpdateReviewRequest request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Попытка обновления отзыва с ID {ReviewId}", reviewId);

            await _updateReviewRequestValidator.ValidateAndThrowAsync(request, cancellationToken);

            var reviewToUpdate = await _reviewRepository.GetReviewByIdAsync(reviewId, cancellationToken);
            if (reviewToUpdate == null)
            {
                _logger.Warning("Отзыв с ID {ReviewId} не найден", reviewId);

                throw new NotFoundException("Отзыва с таким ID не найдено.");
            }

            CheckForUnathorizedAccess(reviewToUpdate, userId);

            var housingResponse = await _housingService.GetByIdAsync(reviewToUpdate.HousingId, cancellationToken);
            var housing = housingResponse?.Housing;
            if (housing == null)
            {
                _logger.Warning("Собственность с ID {HousingId} не найдена при обновлении отзыва с ID {ReviewId}", reviewToUpdate.HousingId, reviewId);

                throw new NotFoundException("Собственности с таким ID не найдено.");
            }

            _mapper.Map(request, reviewToUpdate);

            var existingReview = housing.Reviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (existingReview == null)
            {
                _logger.Warning("Отзыв с ID {ReviewId} не найден в коллекции собственности с ID {HousingId}", reviewId, housing.HousingId);

                throw new NotFoundException("Отзыв не найден в коллекции этого жилья.");
            }

            _mapper.Map(request, existingReview);

            housing.Rating = housing.Reviews.Average(r => r.Rating);

            await _reviewRepository.UpdateAsync(reviewToUpdate, cancellationToken);
            await _housingService.UpdateHousingAsync(housing, cancellationToken);

            _logger.Information("Отзыв с ID {ReviewId} успешно обновлен", reviewId);
        }

        public async Task DeleteReviewAsync(
            Guid reviewId,
            string userId,
            CancellationToken cancellationToken)
        {
            _logger.Information("Попытка удаления отзыва с ID {ReviewId}", reviewId);

            var reviewToDelete = await _reviewRepository.GetReviewByIdAsync(reviewId, cancellationToken);
            if (reviewToDelete == null)
            {
                _logger.Warning("Отзыв с ID {ReviewId} не найден для удаления", reviewId);

                throw new NotFoundException("Отзыва с таким ID не найдено.");
            }

            CheckForUnathorizedAccess(reviewToDelete, userId);

            var housingResponse = await _housingService.GetByIdAsync(reviewToDelete.HousingId, cancellationToken);
            var housing = housingResponse?.Housing;
            if (housing == null)
            {
                _logger.Warning("Собственность с ID {HousingId} не найдена при удалении отзыва с ID {ReviewId}", reviewToDelete.HousingId, reviewId);

                throw new NotFoundException($"Собственности с таким ID не найдено.");
            }

            housing.Reviews.RemoveAll(r => r.ReviewId == reviewToDelete.ReviewId);
            housing.Rating = housing.Reviews.Any() ? housing.Reviews.Average(r => r.Rating) : 0;

            await _reviewRepository.DeleteAsync(reviewId, cancellationToken);
            await _housingService.UpdateHousingAsync(housing, cancellationToken);

            _logger.Information("Отзыв с ID {ReviewId} успешно удален для собственности с ID {HousingId}", reviewId, housing.HousingId);
        }

        private void CheckForUnathorizedAccess(Review reviewToCheck, string userId)
        {
            var parseAttempt = Guid.TryParse(userId, out var userGuid);

            if (!parseAttempt)
            {
                _logger.Warning("Некорректный формат ID комментатора: {UserId}.", userId);

                throw new ArgumentException("Некорректный формат ID комментатора.");
            }

            if (reviewToCheck.UserId != userGuid)
            {
                _logger.Warning("Попытка неавторизованного доступа к комментарию.");

                throw new ArgumentException("Попытка неавторизованного доступа к комментарию.");
            }
        }
    }
}

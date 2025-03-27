using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentIt.Housing.Domain.Contracts.Requests.Reviews;
using RentIt.Housing.Domain.Services;
using Serilog;
using System.Security.Claims;

namespace RentIt.Housing.API.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewsController : Controller
    {
        private readonly ReviewsService _reviewService;

        public ReviewsController(
            ReviewsService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("housing/{housingId}/reviews")]
        public async Task<IActionResult> GetReviewsByHousingId(
            [FromRoute] Guid housingId, 
            CancellationToken cancellationToken)
        {
            Log.Information("Запрос на получение отзывов для жилья с ID {HousingId} в {Time}", housingId, DateTime.UtcNow);

            var reviews = await _reviewService.GetReviewsByHousingIdAsync(housingId, cancellationToken);

            Log.Information("Получено {ReviewCount} отзывов для жилья с ID {HousingId}", reviews.Count(), housingId);

            return Ok(reviews);
        }

        [HttpGet("users/{userId}/reviews")]
        [Authorize]
        public async Task<IActionResult> GetReviewsByUserId(
            [FromRoute] Guid userId,
            CancellationToken cancellationToken)
        {
            Log.Information("Запрос на получение отзывов пользователя с ID {UserId} в {Time}", userId, DateTime.UtcNow);

            var reviews = await _reviewService.GetReviewsByHousingIdAsync(userId, cancellationToken);

            Log.Information("Получено {ReviewCount} отзывов для пользователя с ID {UserId}", reviews.Count(), userId);

            return Ok(reviews);
        }

        [HttpPost("housings/{housingId}")]
        [Authorize]
        public async Task<IActionResult> CreateReview(
            [FromRoute] Guid housingId,
            [FromBody] CreateReviewRequest request, 
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Log.Information("Попытка создания отзыва для жилья с ID {HousingId} пользователем {UserId} в {Time}", housingId, userId, DateTime.UtcNow);

            await _reviewService.AddReviewAsync(userId, housingId, request, cancellationToken);

            Log.Information("Отзыв успешно создан для жилья с ID {HousingId} пользователем {UserId}", housingId, userId);

            return Ok();
        }

        [HttpPut("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(
            [FromRoute] Guid reviewId,
            [FromBody] UpdateReviewRequest request,
            CancellationToken cancellationToken)
        {
            Log.Information("Запрос на обновление отзыва с ID {ReviewId} в {Time}", reviewId, DateTime.UtcNow);

            await _reviewService.UpdateReviewAsync(reviewId, request, cancellationToken);

            Log.Information("Отзыва с ID {ReviewId} успешно обновлён", reviewId);

            return NoContent();
        }

        [HttpDelete("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(
            [FromRoute]Guid reviewId, 
            CancellationToken cancellationToken)
        {
            Log.Information("Запрос на удаление отзыва с ID {ReviewId} в {Time}", reviewId, DateTime.UtcNow);

            await _reviewService.DeleteReviewAsync(reviewId, cancellationToken);

            Log.Information("Отзыв с ID {ReviewId} успешно удалён", reviewId);

            return NoContent();
        }
    }
}
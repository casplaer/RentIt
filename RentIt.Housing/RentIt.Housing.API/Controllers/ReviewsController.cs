using Microsoft.AspNetCore.Mvc;
using RentIt.Housing.Domain.Contracts.Requests.Reviews;
using RentIt.Housing.Domain.Services;
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

        [HttpGet("housing/{housingId}")]
        public async Task<IActionResult> GetReviewsByHousingId(
            [FromRoute] Guid housingId, 
            CancellationToken cancellationToken)
        {
            var reviews = await _reviewService.GetReviewsByHousingIdAsync(housingId, cancellationToken);

            return Ok(reviews);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReviewsByUserId(
            [FromRoute] Guid userId,
            CancellationToken cancellationToken)
        {
            var reviews = await _reviewService.GetReviewsByHousingIdAsync(userId, cancellationToken);

            return Ok(reviews);
        }

        [HttpPost("{housingId}")]
        public async Task<IActionResult> CreateReview(
            [FromRoute] Guid housingId,
            [FromBody] CreateReviewRequest request, 
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _reviewService.AddReviewAsync(userId, housingId, request, cancellationToken);

            return Ok();
        }

        [HttpPut("{reviewId}")]
        public async Task<IActionResult> UpdateReview(
            [FromRoute] Guid reviewId,
            [FromBody] UpdateReviewRequest request,
            CancellationToken cancellationToken)
        {
            await _reviewService.UpdateReviewAsync(reviewId, request, cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(
            Guid id, 
            CancellationToken cancellationToken)
        {
            await _reviewService.DeleteReviewAsync(id, cancellationToken);

            return NoContent();
        }
    }
}
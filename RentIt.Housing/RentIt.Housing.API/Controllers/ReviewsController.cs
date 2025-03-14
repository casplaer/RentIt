using Microsoft.AspNetCore.Mvc;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.Domain.Services;

namespace RentIt.Housing.API.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewsController : Controller
    {
        private readonly ReviewService _reviewService;

        public ReviewsController(
            ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("housing/{housingId}")]
        public async Task<IActionResult> GetReviewsByHousingId(
            Guid housingId, 
            CancellationToken cancellationToken)
        {
            var reviews = await _reviewService.GetReviewsByHousingIdAsync(housingId, cancellationToken);

            return Ok(reviews);
        }

        [HttpPost("{housingId}")]
        public async Task<IActionResult> CreateReview(
            [FromBody] Review review, 
            CancellationToken cancellationToken)
        {
            await _reviewService.AddReviewAsync(review, cancellationToken);

            return Ok(review);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(
            Guid id, 
            [FromBody] Review review,
            CancellationToken cancellationToken)
        {
            if (review == null || id != review.ReviewId)
                return BadRequest();

            await _reviewService.UpdateReviewAsync(review, cancellationToken);
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
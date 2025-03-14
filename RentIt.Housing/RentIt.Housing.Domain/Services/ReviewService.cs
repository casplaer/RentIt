using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;

namespace RentIt.Housing.Domain.Services
{
    public class ReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        }

        public async Task<IEnumerable<Review>> GetReviewsByHousingIdAsync(
            Guid housingId, 
            CancellationToken cancellationToken)
        {
            return await _reviewRepository.GetReviewsByHousingIdAsync(housingId, cancellationToken);
        }

        public async Task AddReviewAsync(Review review, CancellationToken cancellationToken)
        {
            if (review == null)
                throw new ArgumentNullException(nameof(review));

            await _reviewRepository.AddAsync(review, cancellationToken);
        }

        public async Task UpdateReviewAsync(Review review, CancellationToken cancellationToken)
        {
            if (review == null)
                throw new ArgumentNullException(nameof(review));

            await _reviewRepository.UpdateAsync(review, cancellationToken);
        }

        public async Task DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken)
        {
            await _reviewRepository.DeleteAsync(reviewId, cancellationToken);
        }
    }
}

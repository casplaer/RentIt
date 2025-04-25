using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.Domain.Contracts.Requests.Reviews;

namespace RentIt.Housing.Domain.Services.Interfaces
{
    public interface IReviewsService
    {
        Task<IEnumerable<Review>> GetReviewsByHousingIdAsync(
            Guid housingId,
            CancellationToken cancellationToken);

        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken);

        Task AddReviewAsync(
            string userId,
            Guid housingId,
            CreateReviewRequest request,
            CancellationToken cancellationToken);

        Task UpdateReviewAsync(
            Guid reviewId,
            string userId,
            UpdateReviewRequest request,
            CancellationToken cancellationToken);

        Task DeleteReviewAsync(
            Guid reviewId,
            string userId,
            CancellationToken cancellationToken);
    }
}
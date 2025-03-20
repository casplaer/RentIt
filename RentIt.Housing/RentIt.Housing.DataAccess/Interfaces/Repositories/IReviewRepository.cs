using RentIt.Housing.DataAccess.Entities;

namespace RentIt.Housing.DataAccess.Interfaces.Repositories
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetReviewsByHousingIdAsync(Guid housingId, CancellationToken cancellationToken);
        Task<IEnumerable<Review>> GetAllReviewsAsync(CancellationToken cancellationToken);
        Task<Review> GetReviewByIdAsync(Guid reviewId, CancellationToken cancellationToken);
        Task AddAsync(Review review, CancellationToken cancellationToken);
        Task UpdateAsync(Review review, CancellationToken cancellationToken);
        Task DeleteAsync(Guid reviewId, CancellationToken cancellationToken);
    }
}
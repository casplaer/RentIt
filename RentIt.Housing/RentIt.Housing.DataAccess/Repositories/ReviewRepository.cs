using MongoDB.Driver;
using RentIt.Housing.DataAccess.Data;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;

namespace RentIt.Housing.DataAccess.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IMongoCollection<Review> _collection;

        public ReviewRepository(RentItDbContext context)
        {
            _collection = context.Set<Review>("reviews");
        }

        public async Task<IEnumerable<Review>> GetReviewsByHousingIdAsync(Guid housingId, CancellationToken cancellationToken)
        {
            var filter = Builders<Review>.Filter.Eq(r => r.HousingId, housingId);
          
            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Review>> GetAllReviewsAsync(CancellationToken cancellationToken)
        {
            return await _collection.Find(FilterDefinition<Review>.Empty)
                .ToListAsync(cancellationToken);
        }

        public async Task<Review> GetReviewByIdAsync(Guid reviewId, CancellationToken cancellationToken)
        {
            var filter = Builders<Review>.Filter.Eq(r => r.ReviewId, reviewId);
            
            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(Review review, CancellationToken cancellationToken)
        {
            await _collection.InsertOneAsync(review, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(Review review, CancellationToken cancellationToken)
        {
            var filter = Builders<Review>.Filter.Eq(r => r.ReviewId, review.ReviewId);
            
            await _collection.ReplaceOneAsync(filter, review, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(Guid reviewId, CancellationToken cancellationToken)
        {
            var filter = Builders<Review>.Filter.Eq(r => r.ReviewId, reviewId);
            
            await _collection.DeleteOneAsync(filter, cancellationToken);
        }
    }
}
using MongoDB.Driver;
using RentIt.Housing.DataAccess.Data;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;

namespace RentIt.Housing.DataAccess.Repositories
{
    public class HousingImageRepository : IHousingImageRepository
    {
        private readonly IMongoCollection<HousingImage> _collection;

        public HousingImageRepository(RentItDbContext context)
        {
            _collection = context.Set<HousingImage>("housing_images");
        }

        public async Task<IEnumerable<HousingImage>> GetImagesByHousingIdAsync(Guid housingId, CancellationToken cancellationToken)
        {
            var filter = Builders<HousingImage>.Filter.Eq(pi => pi.HousingId, housingId);
            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }

        public async Task<HousingImage> GetHousingImageByIdAsync(Guid housingImageId, CancellationToken cancellationToken)
        {
            var filter = Builders<HousingImage>.Filter.Eq(hi => hi.ImageId, housingImageId);
            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(HousingImage image, CancellationToken cancellationToken)
        {
            await _collection.InsertOneAsync(image, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(HousingImage image, CancellationToken cancellationToken)
        {
            var filter = Builders<HousingImage>.Filter.Eq(pi => pi.ImageId, image.ImageId);
            await _collection.ReplaceOneAsync(filter, image, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(Guid imageId, CancellationToken cancellationToken)
        {
            var filter = Builders<HousingImage>.Filter.Eq(pi => pi.ImageId, imageId);
            await _collection.DeleteOneAsync(filter, cancellationToken);
        }
    }
}
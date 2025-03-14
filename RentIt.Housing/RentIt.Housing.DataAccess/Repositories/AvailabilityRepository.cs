using MongoDB.Driver;
using RentIt.Housing.DataAccess.Data;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;

namespace RentIt.Housing.DataAccess.Repositories
{
    public class AvailabilityRepository : IAvailabilityRepository
    {
        private readonly IMongoCollection<Availability> _collection;

        public AvailabilityRepository(RentItDbContext context)
        {
            _collection = context.Set<Availability>("availabilities");
        }

        public async Task<Availability> GetAvailabilityByIdAsync(Guid availabilityId, CancellationToken cancellationToken)
        {
            var filter = Builders<Availability>.Filter.Eq(a => a.AvailabilityId, availabilityId);
            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<Availability>> GetAvailabilitiesByHousingIdAsync(Guid housingId, CancellationToken cancellationToken)
        {
            var filter = Builders<Availability>.Filter.Eq(a => a.HousingId, housingId);
            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Availability availability, CancellationToken cancellationToken)
        {
            await _collection.InsertOneAsync(availability, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(Availability availability, CancellationToken cancellationToken)
        {
            var filter = Builders<Availability>.Filter.Eq(a => a.AvailabilityId, availability.AvailabilityId);
            await _collection.ReplaceOneAsync(filter, availability, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(Guid availabilityId, CancellationToken cancellationToken)
        {
            var filter = Builders<Availability>.Filter.Eq(a => a.AvailabilityId, availabilityId);
            await _collection.DeleteOneAsync(filter, cancellationToken);
        }
    }
}
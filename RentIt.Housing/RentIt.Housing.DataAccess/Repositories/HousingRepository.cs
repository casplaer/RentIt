using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RentIt.Housing.DataAccess.Data;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.DataAccess.Specifications;

namespace RentIt.Housing.DataAccess.Repositories
{
    public class HousingRepository : IHousingRepository
    {
        private readonly IMongoCollection<HousingEntity> _collection;

        public HousingRepository(RentItDbContext context)
        {
            _collection = context.Set<HousingEntity>("housings");
        }

        public async Task<HousingEntity> GetByIdAsync(Guid housingId, CancellationToken cancellationToken)
        {
            var filter = Builders<HousingEntity>.Filter.Eq(p => p.HousingId, housingId);
            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<HousingEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _collection.Find(FilterDefinition<HousingEntity>.Empty)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<HousingEntity>> SearchAsync(Specification<HousingEntity> specification, CancellationToken cancellationToken)
        {
            var query = _collection.AsQueryable();

            query = ApplySpecification(specification);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task AddAsync(HousingEntity housing, CancellationToken cancellationToken)
        {
            await _collection.InsertOneAsync(housing, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(HousingEntity housing, CancellationToken cancellationToken)
        {
            var filter = Builders<HousingEntity>.Filter.Eq(p => p.HousingId, housing.HousingId);
            await _collection.ReplaceOneAsync(filter, housing, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(Guid housingId, CancellationToken cancellationToken)
        {
            var filter = Builders<HousingEntity>.Filter.Eq(p => p.HousingId, housingId);
            await _collection.DeleteOneAsync(filter, cancellationToken);
        }

        private IQueryable<HousingEntity> ApplySpecification(
            Specification<HousingEntity> specification)
        {
            return SpecificationEvaluator.GetQuery(
                _collection.AsQueryable(),
                specification
                );
        }
    }
}
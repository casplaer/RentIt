using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Specifications;
using RentIt.Housing.DataAccess.Specifications.Housing;

namespace RentIt.Housing.DataAccess.Interfaces.Repositories
{
    public interface IHousingRepository
    {
        Task<HousingEntity> GetByIdAsync(Guid housingId, CancellationToken cancellationToken);
        Task<IEnumerable<HousingEntity>> SearchAsync(Specification<HousingEntity> specification, CancellationToken cancellationToken);
        Task AddAsync(HousingEntity housing, CancellationToken cancellationToken);
        Task UpdateAsync(HousingEntity housing, CancellationToken cancellationToken);
        Task DeleteAsync(Guid housingId, CancellationToken cancellationToken);
    }
}
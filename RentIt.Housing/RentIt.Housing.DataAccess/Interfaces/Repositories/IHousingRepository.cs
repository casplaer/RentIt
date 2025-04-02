using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Specifications;

namespace RentIt.Housing.DataAccess.Interfaces.Repositories
{
    public interface IHousingRepository
    {
        Task<HousingEntity> GetByIdAsync(Guid housingId, CancellationToken cancellationToken);
        Task<IEnumerable<HousingEntity>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<HousingEntity>> GetAllUnpublishedAsync(CancellationToken cancellationToken);
        Task<IEnumerable<HousingEntity>> SearchAsync(Specification<HousingEntity> specification, CancellationToken cancellationToken);
        Task AddAsync(HousingEntity housing, CancellationToken cancellationToken);
        Task UpdateAsync(HousingEntity housing, CancellationToken cancellationToken);
        Task DeleteAsync(Guid housingId, CancellationToken cancellationToken);
    }
}
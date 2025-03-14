using RentIt.Housing.DataAccess.Entities;

namespace RentIt.Housing.DataAccess.Interfaces.Repositories
{
    public interface IAvailabilityRepository
    {
        Task<IEnumerable<Availability>> GetAvailabilitiesByHousingIdAsync(Guid housingId, CancellationToken cancellationToken);
        Task<Availability> GetAvailabilityByIdAsync(Guid availabilityId, CancellationToken cancellationToken);
        Task AddAsync(Availability availability, CancellationToken cancellationToken);
        Task UpdateAsync(Availability availability, CancellationToken cancellationToken);
        Task DeleteAsync(Guid availabilityId, CancellationToken cancellationToken);
    }
}
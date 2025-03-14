using RentIt.Housing.DataAccess.Entities;

namespace RentIt.Housing.DataAccess.Interfaces.Repositories
{
    public interface IHousingImageRepository
    {
        Task<IEnumerable<HousingImage>> GetImagesByHousingIdAsync(Guid housingId, CancellationToken cancellationToken);
        Task<HousingImage> GetHousingImageByIdAsync(Guid housingImageId, CancellationToken cancellationToken);
        Task AddAsync(HousingImage image, CancellationToken cancellationToken);
        Task UpdateAsync(HousingImage image, CancellationToken cancellationToken);
        Task DeleteAsync(Guid imageId, CancellationToken cancellationToken);
    }
}
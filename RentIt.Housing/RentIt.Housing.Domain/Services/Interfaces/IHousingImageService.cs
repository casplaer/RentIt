using Microsoft.AspNetCore.Http;
using RentIt.Housing.DataAccess.Entities;

namespace RentIt.Housing.Domain.Services.Interfaces
{
    public interface IHousingImageService
    {
        Task<IEnumerable<HousingImage>> GetImagesByHousingIdAsync(Guid housingId, CancellationToken cancellationToken);

        Task<List<HousingImage>> UploadImagesAsync(
            Guid housingId,
            IEnumerable<IFormFile> images,
            CancellationToken cancellationToken);

        Task<List<HousingImage>> UpdateImagesAsync(
            Guid housingId,
            List<IFormFile>? addedImages,
            List<string>? removedImages,
            CancellationToken cancellationToken);

        Task DeleteImageAsync(Guid imageId, CancellationToken cancellationToken);

        Task ClearImagesAsync(IEnumerable<HousingImage> images, CancellationToken cancellationToken);
    }

}

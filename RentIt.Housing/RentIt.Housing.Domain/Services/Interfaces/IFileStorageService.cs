using Microsoft.AspNetCore.Http;

namespace RentIt.Housing.Domain.Services.Interfaces
{
    public interface IFileStorageService
    {
        void ValidateImageFile(IFormFile image);
        Task<string> SaveFileAsync(IFormFile file, CancellationToken cancellationToken);
        bool DeleteFile(string fileName);
    }
}
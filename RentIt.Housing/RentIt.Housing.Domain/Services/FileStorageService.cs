using Microsoft.AspNetCore.Http;
using Serilog;

namespace RentIt.Housing.Domain.Services
{
    public class FileStorageService
    {
        private readonly ILogger _logger;

        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "housing_images");

        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/bmp" };

        public FileStorageService(ILogger logger)
        {
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }

            _logger = logger;
        }

        public void ValidateImageFile(IFormFile image)
        {
            var fileExtension = Path.GetExtension(image.FileName).ToLower();
            var contentType = image.ContentType.ToLower();

            if (!_allowedExtensions.Contains(fileExtension))
            {
                _logger.Error("Попытка загрузки файла с неподдерживаемым расширением: {FileName}", image.FileName);

                throw new ArgumentException("Загруженный файл не является изображением допустимого формата.", nameof(image));
            }

            if (!_allowedMimeTypes.Contains(contentType))
            {
                _logger.Error("Попытка загрузки файла с неподдерживаемым MIME-типом: {ContentType}", contentType);

                throw new ArgumentException("Загруженный файл не является изображением допустимого типа.", nameof(image));
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using ( var stream = new FileStream(filePath, FileMode.Create) )
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            return $"/uploads/housing_images/{fileName}";
        }

        public bool DeleteFile(string fileName)
        {
            var filePath = Path.Combine(_uploadPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
    }
}
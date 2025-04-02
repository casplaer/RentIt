using Microsoft.AspNetCore.Http;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using Serilog;

namespace RentIt.Housing.Domain.Services
{
    public class HousingImageService
    {
        private readonly IHousingImageRepository _imageRepository;
        private readonly FileStorageService _fileStorageService;
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "housing_images");
        private readonly ILogger _logger;

        public HousingImageService(
            IHousingImageRepository imageRepository,
            FileStorageService fileStorageService,
            ILogger logger)
        {
            _imageRepository = imageRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<IEnumerable<HousingImage>> GetImagesByHousingIdAsync(Guid housingId, CancellationToken cancellationToken)
        {
            _logger.Information("Получение изображений для собственности с ID {HousingId}", housingId);

            var images = await _imageRepository.GetImagesByHousingIdAsync(housingId, cancellationToken);

            _logger.Information("Найдено {ImageCount} изображений для собственности с ID {HousingId}", images.Count(), housingId);

            return images;
        }

        public async Task<List<HousingImage>> UploadImagesAsync(
            Guid housingId,
            IEnumerable<IFormFile> images,
            CancellationToken cancellationToken)
        {
            _logger.Information("Начало загрузки изображений для собственности с ID {HousingId}", housingId);

            if (images == null || !images.Any())
            {
                _logger.Warning("Попытка загрузки изображений для собственности с ID {HousingId}: изображения не предоставлены", housingId);

                throw new ArgumentException("Не были загружены изображения.", nameof(images));
            }

            var uploadedImages = new List<HousingImage>();
            int order = 1;

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(image.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        _logger.Error("Попытка загрузки файла с неподдерживаемым расширением: {FileName}", image.FileName);

                        throw new ArgumentException("Загруженный файл не является изображением допустимого формата.", nameof(images));
                    }

                    var contentType = image.ContentType.ToLower();
                    var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };

                    if (!allowedMimeTypes.Contains(contentType))
                    {
                        _logger.Error("Попытка загрузки файла с неподдерживаемым MIME-типом: {ContentType}", contentType);

                        throw new ArgumentException("Загруженный файл не является изображением допустимого типа.", nameof(images));
                    }

                    var savedFileName = await _fileStorageService.SaveFileAsync(image, _uploadPath, cancellationToken);
                    var imageUrl = $"/uploads/housing_images/{savedFileName}";

                    var housingImage = new HousingImage
                    {
                        ImageId = Guid.NewGuid(),
                        HousingId = housingId,
                        ImageUrl = imageUrl,
                        Order = order++
                    };

                    await _imageRepository.AddAsync(housingImage, cancellationToken);

                    _logger.Information("Изображение успешно добавлено с ID {ImageId} для собственности с ID {HousingId}", housingImage.ImageId, housingId);

                    uploadedImages.Add(housingImage);
                }
            }

            _logger.Information("Загрузка изображений завершена для собственности с ID {HousingId}. Загружено изображений: {Count}", housingId, uploadedImages.Count);
            return uploadedImages;
        }

        public async Task<List<HousingImage>> UpdateImagesAsync(
            Guid housingId,
            List<IFormFile>? addedImages,
            List<string>? removedImages,
            CancellationToken cancellationToken)
        {
            _logger.Information("Начало обновления изображений для собственности с ID {HousingId}", housingId);

            var images = (await _imageRepository.GetImagesByHousingIdAsync(housingId, cancellationToken)).ToList();

            if (addedImages != null && addedImages.Any())
            {
                int order = images.Any() ? images.Max(img => img.Order) : 1;
                foreach (var file in addedImages)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        _logger.Error("Добавление изображения не выполнено. Неподдерживаемое расширение файла: {FileName}", file.FileName);
                        throw new ArgumentException("Загруженный файл не является изображением допустимого формата.", nameof(addedImages));
                    }

                    var contentType = file.ContentType.ToLower();
                    var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };

                    if (!allowedMimeTypes.Contains(contentType))
                    {
                        _logger.Error("Добавление изображения не выполнено. Неподдерживаемый MIME-тип файла: {ContentType}", file.ContentType);
                        throw new ArgumentException("Загруженный файл не является изображением допустимого типа.", nameof(addedImages));
                    }

                    var savedFileName = await _fileStorageService.SaveFileAsync(file, _uploadPath, cancellationToken);
                    var fileName = savedFileName;
                    var imageUrl = $"/uploads/housing_images/{fileName}";

                    var newImage = new HousingImage
                    {
                        ImageId = Guid.NewGuid(),
                        HousingId = housingId,
                        ImageUrl = imageUrl,
                        Order = order++,
                    };

                    await _imageRepository.AddAsync(newImage, cancellationToken);

                    _logger.Information("Новое изображение добавлено с ID {ImageId} для собственности с ID {HousingId}", newImage.ImageId, housingId);

                    images.Add(newImage);
                }
            }

            if (removedImages != null && removedImages.Any())
            {
                foreach (var relativePath in removedImages)
                {
                    var fileName = Path.GetFileName(relativePath);
                    var imageRecord = images.FirstOrDefault(img => Path.GetFileName(img.ImageUrl) == fileName);

                    if (imageRecord != null)
                    {
                        _logger.Information("Удаление изображения с ID {ImageId} для собственности с ID {HousingId}", imageRecord.ImageId, housingId);

                        await DeleteImageAsync(imageRecord.ImageId, cancellationToken);

                        images.Remove(imageRecord);
                    }
                    else
                    {
                        _logger.Warning("Изображение для удаления не найдено по пути: {RelativePath}", relativePath);
                    }
                }
            }

            _logger.Information("Обновление изображений завершено для собственности с ID {HousingId}. Текущее количество изображений: {Count}", housingId, images.Count);

            return images;
        }

        public async Task DeleteImageAsync(Guid imageId, CancellationToken cancellationToken)
        {
            _logger.Information("Попытка удаления изображения с ID {ImageId}", imageId);
            
            var imageToDelete = await _imageRepository.GetHousingImageByIdAsync(imageId, cancellationToken);

            if (imageToDelete == null)
            {
                _logger.Error("Изображение с ID {ImageId} не найдено", imageId);

                throw new ArgumentException("Изображение не найдено.", nameof(imageId));
            }

            var fileName = Path.GetFileName(imageToDelete.ImageUrl);
            var filePath = Path.Combine(_uploadPath, fileName);

            bool fileDeleted = _fileStorageService.DeleteFile(filePath);

            if (fileDeleted)
            {
                _logger.Information("Файл изображения удален с диска: {FilePath}", filePath);
            }
            else
            {
                _logger.Warning("Файл изображения не найден на диске: {FilePath}", filePath);
            }

            await _imageRepository.DeleteAsync(imageId, cancellationToken);

            _logger.Information("Запись изображения с ID {ImageId} удалена из базы данных", imageId);
        }
    }
}
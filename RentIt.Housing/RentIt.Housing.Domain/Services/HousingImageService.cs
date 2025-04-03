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
                if (image.Length <= 0)
                {
                    continue;
                }

                _fileStorageService.ValidateImageFile(image);

                var imageUrl = await _fileStorageService.SaveFileAsync(image, cancellationToken);

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
                    _fileStorageService.ValidateImageFile(file);

                    var imageUrl = await _fileStorageService.SaveFileAsync(file, cancellationToken);

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

                    if (imageRecord == null)
                    {
                        _logger.Warning("Изображение для удаления не найдено по пути: {RelativePath}", relativePath);
                    }
                    else
                    {
                        _logger.Information("Удаление изображения с ID {ImageId} для собственности с ID {HousingId}", imageRecord.ImageId, housingId);

                        await DeleteImageAsync(imageRecord.ImageId, cancellationToken);

                        images.Remove(imageRecord);
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

            var fileDeleted = _fileStorageService.DeleteFile(fileName);

            if (fileDeleted)
            {
                _logger.Information("Файл изображения удален с диска: {FilePath}", fileName);
            }
            else
            {
                _logger.Warning("Файл изображения не найден на диске: {FilePath}", fileName);
            }

            await _imageRepository.DeleteAsync(imageId, cancellationToken);

            _logger.Information("Запись изображения с ID {ImageId} удалена из базы данных", imageId);
        }

        public async Task ClearImagesAsync(IEnumerable<HousingImage> images, CancellationToken cancellationToken)
        {
            var tasks = images.Select(async img =>
            {
                _logger.Information("Удаление изображения с ID {ImageId}", img.ImageId);

                DeleteImageAsync(img.ImageId, cancellationToken);
            }).ToList();

            await Task.WhenAll(tasks);
        }
    }
}
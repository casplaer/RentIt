using Microsoft.AspNetCore.Http;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;

namespace RentIt.Housing.Domain.Services
{
    public class HousingImageService
    {
        private readonly IHousingImageRepository _imageRepository;
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "housing_images");

        public HousingImageService(IHousingImageRepository imageRepository)
        {
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        }

        public async Task<IEnumerable<HousingImage>> GetImagesByHousingIdAsync(
            Guid housingId, 
            CancellationToken cancellationToken)
        {
            return await _imageRepository.GetImagesByHousingIdAsync(housingId, cancellationToken);
        }

        public async Task<List<HousingImage>> UploadImagesAsync(
            Guid housingId,
            IEnumerable<IFormFile> images,
            CancellationToken cancellationToken)
        {
            if (images == null || images.Count() == 0)
            {
                throw new ArgumentException("No images were uploaded.", nameof(images));
            }

            var uploadedImages = new List<HousingImage>();

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }

            int order = 1;
            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(image.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        throw new ArgumentException("The uploaded file is not a valid image format.", nameof(images));
                    }

                    var contentType = image.ContentType.ToLower();
                    var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };

                    if (!allowedMimeTypes.Contains(contentType))
                    {
                        throw new ArgumentException("The uploaded file is not a valid image type.", nameof(images));
                    }

                    var imageName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                    var imagePath = Path.Combine(_uploadPath, imageName);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream, cancellationToken);
                    }

                    var housingImage = new HousingImage
                    {
                        ImageId = Guid.NewGuid(),
                        HousingId = housingId,
                        ImageUrl = $"/uploads/housing_images/{imageName}",
                        Order = order++
                    };

                    await _imageRepository.AddAsync(housingImage, cancellationToken);
                    uploadedImages.Add(housingImage);
                }
            }

            return uploadedImages;
        }

        public async Task<List<HousingImage>> UpdateImagesAsync(
            Guid housingId,
            List<IFormFile>? addedImages,
            List<string>? removedImages,
            CancellationToken cancellationToken)
        {
            List<HousingImage> images = (await _imageRepository.GetImagesByHousingIdAsync(housingId, cancellationToken)).ToList();

            if (addedImages != null && addedImages.Any())
            {
                int order = images.Any() ? images.Max(img => img.Order) : 1;
                foreach (var file in addedImages)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        throw new ArgumentException("The uploaded file is not a valid image format.", nameof(addedImages));
                    }

                    var contentType = file.ContentType.ToLower();
                    var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };
                    if (!allowedMimeTypes.Contains(contentType))
                    {
                        throw new ArgumentException("The uploaded file is not a valid image type.", nameof(addedImages));
                    }

                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(_uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream, cancellationToken);
                    }

                    var newImage = new HousingImage
                    {
                        ImageId = Guid.NewGuid(),
                        HousingId = housingId,
                        ImageUrl = $"/uploads/{fileName}",
                        Order = order++,
                    };

                    await _imageRepository.AddAsync(newImage, cancellationToken);
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
                        await DeleteImageAsync(imageRecord.ImageId, cancellationToken);
                        images.Remove(imageRecord);
                    }
                }
            }

            return images;
        }

        public async Task DeleteImageAsync(
            Guid imageId, 
            CancellationToken cancellationToken)
        {
            var imageToDelete = await _imageRepository.GetHousingImageByIdAsync(imageId, cancellationToken);

            if (imageToDelete == null)
            {
                throw new ArgumentException("Изображение не найдено.", nameof(imageId));
            }

            var fileName = Path.GetFileName(imageToDelete.ImageUrl);
            var filePath = Path.Combine(_uploadPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            await _imageRepository.DeleteAsync(imageId, cancellationToken);
        }
    }
}
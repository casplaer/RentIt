using AutoMapper;
using DnsClient.Internal;
using FluentValidation;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Enums;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.DataAccess.Specifications.Housing;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Contracts.Responses.Housing;
using RentIt.Housing.Domain.Exceptions;
using Serilog;

namespace RentIt.Housing.Domain.Services
{
    public class HousingService
    {
        private readonly IHousingRepository _housingRepository;
        private readonly HousingImageService _imageService;
        private readonly UserIntegrationService _userIntegrationService;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateHousingRequest> _createHousingRequestValidator;
        private readonly IValidator<GetFilteredHousingsRequest> _getFilteredHousingRequestValidator;
        private readonly IValidator<UpdateHousingRequest> _updateHousingRequestValidator;
        private readonly SpamProfanityFilterService _filterService;
        private readonly Serilog.ILogger _logger;

        public HousingService(
            IHousingRepository housingRepository,
            IMapper mapper,
            IValidator<CreateHousingRequest> createHousingRequestValidator,
            IValidator<GetFilteredHousingsRequest> getFilteredHousingRequestValidator,
            IValidator<UpdateHousingRequest> updateHousingRequestValidator,
            HousingImageService imageService,
            UserIntegrationService userIntegrationService,
            SpamProfanityFilterService filterService,
            Serilog.ILogger logger)
        {
            _housingRepository = housingRepository;
            _mapper = mapper;
            _createHousingRequestValidator = createHousingRequestValidator;
            _getFilteredHousingRequestValidator = getFilteredHousingRequestValidator;
            _updateHousingRequestValidator = updateHousingRequestValidator;
            _imageService = imageService;
            _userIntegrationService = userIntegrationService;
            _filterService = filterService;
            _logger = logger;
        }

        public async Task<GetHousingByIdResponse> GetByIdAsync(
            Guid housingId, 
            CancellationToken cancellationToken)
        {
            _logger.Information("Получение собственности с ID {HousingId}", housingId);

            var housing = await _housingRepository.GetByIdAsync(housingId, cancellationToken);

            if (housing == null)
            {
                _logger.Warning("Собственность с ID {HousingId} не найдена", housingId);
                throw new NotFoundException("Собственности с таким ID не найдено.");
            }

            var ownerInfo = await _userIntegrationService.GetUserInfoAsync(housing.OwnerId);

            var housingToReturn = new GetHousingByIdResponse(
                housing,
                ownerInfo);

            _logger.Information("Собственность с ID {HousingId} успешно получена", housingId);

            return housingToReturn;
        }

        public async Task<IEnumerable<HousingEntity>> SearchAsync(
            GetFilteredHousingsRequest request, 
            CancellationToken cancellationToken)
        {
            _logger.Information("Поиск недвижимости с параметрами: " +
                "Название: {Title}, Адрес: {Address}, Город: {City}, Страна: {Country}, " +
                "Цена за ночь: {PricePerNight}, Количество комнат: {NumberOfRooms}, " +
                "Рейтинг: {Rating}, Статус: {Status}, Дата начала: {StartDate}, Дата окончания: {EndDate}, " +
                "Страница: {Page}, Размер страницы: {PageSize}",
                request.Title ?? "Не задано",
                request.Address ?? "Не задано",
                request.City ?? "Не задано",
                request.Country ?? "Не задано",
                request.PricePerNight?.ToString() ?? "Не задано",
                request.NumberOfRooms?.ToString() ?? "Не задано",
                request.Rating?.ToString() ?? "Не задано",
                request.Status?.ToString() ?? "Не задано",
                request.EstimatedEndDate?.ToString("yyyy-MM-dd") ?? "Не задано",
                request.Page,
                request.PageSize);

            await _getFilteredHousingRequestValidator.ValidateAndThrowAsync(request, cancellationToken);

            var specification = new SearchHousingSpecification(
                title: request.Title,
                address: request.Address,
                city: request.City,
                country: request.Country,
                pricePerNight: request.PricePerNight,
                numberOfRooms: request.NumberOfRooms,
                rating: request.Rating,
                status: request.Status,
                estimatedEndDate: request.EstimatedEndDate,
                page: request.Page,
                pageSize: request.PageSize
            );

            var housings = await _housingRepository.SearchAsync(specification, cancellationToken);

            _logger.Information("Найдено {HousingCount} объектов недвижимости, соответствующих критериям поиска.", housings.Count());

            return housings;
        }

        public async Task AddHousingAsync(
            string ownerId,
            CreateHousingRequest request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Добавление новой собственности для владельца {OwnerId}.", ownerId);

            await _createHousingRequestValidator.ValidateAndThrowAsync(request, cancellationToken);

            var parseAttempt = Guid.TryParse(ownerId, out var ownerGuid);

            if (!parseAttempt)
            {
                _logger.Error("Некорректный формат ID владельца: {OwnerId}.", ownerId);

                throw new ArgumentException("Некорректный формат ID.");
            }

            var housing = _mapper.Map<HousingEntity>(request, opt =>
            {
                opt.Items["ownerId"] = ownerGuid;
            });

            var imagesUploaded = request.Images != null && request.Images.Count() != 0;

            if (imagesUploaded)
            {
                _logger.Information("Загрузка {ImageCount} изображений для собственности.", request.Images.Count());

                housing.Images = await _imageService.UploadImagesAsync(housing.HousingId, request.Images, cancellationToken);
            }

            await _housingRepository.AddAsync(housing, cancellationToken);

            _logger.Information("Собственность с ID {HousingId} успешно добавлена.", housing.HousingId);
        }

        public async Task UpdateHousingAsync(
            Guid housingId,
            string userId,
            UpdateHousingRequest request, 
            CancellationToken cancellationToken)
        {
            _logger.Information("Обновление собственности с ID {HousingId}.", housingId);

            await _updateHousingRequestValidator.ValidateAndThrowAsync(request, cancellationToken);

            var housingToUpdate = await _housingRepository.GetByIdAsync(housingId, cancellationToken);

            if(housingToUpdate == null )
            {
                _logger.Warning("Собственность с ID {HousingId} не найдена для обновления.", housingId);

                throw new NotFoundException($"Собственность с ID {housingId} не найдена.");
            }

            CheckForUnathorizedAccess(housingToUpdate, userId);

            _mapper.Map(request, housingToUpdate);

            housingToUpdate.Images = await _imageService.UpdateImagesAsync(housingId, request.AddedImages, request.RemovedImages, cancellationToken);

            housingToUpdate.UpdatedAt = DateTime.UtcNow;

            await _housingRepository.UpdateAsync(housingToUpdate, cancellationToken);

            _logger.Information("Собственность с ID {HousingId} успешно обновлена.", housingId);
        }

        public async Task UpdateHousingAsync(
            HousingEntity housing,
            CancellationToken cancellationToken
            )
        {
            _logger.Information("Обновление собственности с ID {HousingId}.", housing.HousingId);

            housing.UpdatedAt = DateTime.UtcNow;

            await _housingRepository.UpdateAsync(housing, cancellationToken);

            _logger.Information("Собственность с ID {HousingId} успешно обновлена.", housing.HousingId);
        }

        public async Task DeleteHousingAsync(
            Guid housingId, 
            string userId,
            CancellationToken cancellationToken)
        {
            _logger.Information("Удаление собственности с ID {HousingId}.", housingId);

            var housingToDelete = await _housingRepository.GetByIdAsync(housingId, cancellationToken);

            if (housingToDelete == null)
            {
                _logger.Warning("Собственность для удаления с ID {housingId} не найдена.", housingId);

                throw new NotFoundException("Собственность для удаления не найдена.");
            }

            CheckForUnathorizedAccess(housingToDelete, userId);

            await _imageService.ClearImagesAsync(housingToDelete.Images, cancellationToken);

            await _housingRepository.DeleteAsync(housingId, cancellationToken);

            _logger.Information("Собственность с ID {HousingId} успешно удалена.", housingId);
        }

        public async Task CheckUnpublishedHousingsForSpamAsync(
            CancellationToken cancellationToken)
        {
            _logger.Information("Проверка неподтвержденных объектов недвижимости на наличие спама.");

            var unpublishedHousings = await _housingRepository.GetAllUnpublishedAsync(cancellationToken);

            var tasks = unpublishedHousings.Select(housing => ProcessHousingForSpamAsync(housing, cancellationToken));

            await Task.WhenAll(tasks);

            _logger.Information("Проверка неподтвержденных объектов недвижимости на спам завершена.");
        }

        private async Task ProcessHousingForSpamAsync(HousingEntity housing, CancellationToken cancellationToken)
        {
            var isSpam = _filterService.ContainsSpamOrProfanity(housing.Description) || _filterService.ContainsSpamOrProfanity(housing.Title);

            if (isSpam)
            {
                housing.Status = HousingStatus.Rejected;

                _logger.Warning("Собственность с ID {HousingId} помечена как спам.", housing.HousingId);
            }
            else
            {
                housing.Status = HousingStatus.Available;

                _logger.Information("Собственность с ID {HousingId} помечена как доступная.", housing.HousingId);
            }

            await _housingRepository.UpdateAsync(housing, cancellationToken);
        }

        private void CheckForUnathorizedAccess(HousingEntity housingToCheck, string userId)
        {
            var parseAttempt = Guid.TryParse(userId, out var userGuid);

            if (!parseAttempt)
            {
                _logger.Warning("Некорректный формат ID комментатора: {UserId}.", userId);

                throw new ArgumentException("Некорректный формат ID владельца.");
            }

            if (housingToCheck.OwnerId != userGuid)
            {
                _logger.Warning("Попытка неавторизованного доступа к комментарию.");

                throw new ArgumentException("Попытка неавторизованного доступа к собственности.");
            }
        }
    }
}
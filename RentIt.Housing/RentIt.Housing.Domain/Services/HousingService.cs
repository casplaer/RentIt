using AutoMapper;
using FluentValidation;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Enums;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.DataAccess.Specifications.Housing;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Contracts.Responses.Housing;
using RentIt.Housing.Domain.Exceptions;

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

        public HousingService(
            IHousingRepository housingRepository,
            IMapper mapper,
            IValidator<CreateHousingRequest> createHousingRequestValidator,
            IValidator<GetFilteredHousingsRequest> getFilteredHousingRequestValidator,
            IValidator<UpdateHousingRequest> updateHousingRequestValidator,
            HousingImageService imageService,
            UserIntegrationService userIntegrationService,
            SpamProfanityFilterService filterService)
        {
            _housingRepository = housingRepository;
            _mapper = mapper;
            _createHousingRequestValidator = createHousingRequestValidator;
            _getFilteredHousingRequestValidator = getFilteredHousingRequestValidator;
            _updateHousingRequestValidator = updateHousingRequestValidator;
            _imageService = imageService;
            _userIntegrationService = userIntegrationService;
            _filterService = filterService;
        }

        public async Task<GetHousingByIdResponse> GetByIdAsync(
            Guid housingId, 
            CancellationToken cancellationToken)
        {
            var housing = await _housingRepository.GetByIdAsync(housingId, cancellationToken);

            if(housing == null)
            {
                throw new NotFoundException("Собственности с таким ID не найдено.");
            }

            var ownerInfo = await _userIntegrationService.GetUserInfoAsync(housing.OwnerId);

            var housingToReturn = new GetHousingByIdResponse(
                housing,
                ownerInfo);

            return housingToReturn;
        }

        public async Task<IEnumerable<HousingEntity>> SearchAsync(
            GetFilteredHousingsRequest request, 
            CancellationToken cancellationToken)
        {
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
                startDate: request.StartDate,
                endDate: request.EndDate,
                page: request.Page,
                pageSize: request.PageSize
            );

            return await _housingRepository.SearchAsync(specification, cancellationToken);
        }

        public async Task AddHousingAsync(
            string ownerId,
            CreateHousingRequest request,
            CancellationToken cancellationToken)
        {
            await _createHousingRequestValidator.ValidateAndThrowAsync(request, cancellationToken);

            if (!Guid.TryParse(ownerId, out var ownerGuid))
            {
                throw new ArgumentException("Некорректный формат ID.");
            }

            var housing = _mapper.Map<HousingEntity>(request, opt =>
            {
                opt.Items["ownerId"] = ownerGuid;
            });

            if(request.Images != null && request.Images.Count() != 0)
            {
                housing.Images = await _imageService.UploadImagesAsync(housing.HousingId, request.Images, cancellationToken);
            }

            await _housingRepository.AddAsync(housing, cancellationToken);
        }

        public async Task UpdateHousingAsync(
            Guid housingId,
            UpdateHousingRequest request, 
            CancellationToken cancellationToken)
        {
            await _updateHousingRequestValidator.ValidateAndThrowAsync(request, cancellationToken);

            var housingToUpdate = await _housingRepository.GetByIdAsync(housingId, cancellationToken);

            if(housingToUpdate == null )
            {
                throw new NotFoundException($"Собственность с ID {housingId} не найдена.");
            }

            _mapper.Map(request, housingToUpdate);

            housingToUpdate.Images = await _imageService.UpdateImagesAsync(housingId, request.AddedImages, request.RemovedImages, cancellationToken);

            housingToUpdate.UpdatedAt = DateTime.UtcNow;

            await _housingRepository.UpdateAsync(housingToUpdate, cancellationToken);
        }

        public async Task UpdateHousingAsync(
            HousingEntity housing,
            CancellationToken cancellationToken
            )
        {
            housing.UpdatedAt = DateTime.UtcNow;

            await _housingRepository.UpdateAsync(housing, cancellationToken);
        }

        public async Task DeleteHousingAsync(
            Guid housingId, 
            CancellationToken cancellationToken)
        {
            var housingToDelete = await _housingRepository.GetByIdAsync(housingId, cancellationToken);

            foreach(var img in housingToDelete.Images)
            {
                await _imageService.DeleteImageAsync(img.ImageId, cancellationToken);
            }

            await _housingRepository.DeleteAsync(housingId, cancellationToken);
        }

        public async Task CheckUnpublishedHousingsForSpamAsync(
            CancellationToken cancellationToken)
        {
            var unpublishedHousings = (await _housingRepository.GetAllAsync(cancellationToken))
                                        .Where(h => h.Status == HousingStatus.Unpublished);

            foreach (var housing in unpublishedHousings)
            {
                var isSpam = _filterService.ContainsSpamOrProfanity(housing.Description) && _filterService.ContainsSpamOrProfanity(housing.Title);

                if (isSpam)
                {
                    housing.Status = HousingStatus.Rejected;
                }
                else
                {
                    housing.Status = HousingStatus.Available;
                }

                await _housingRepository.UpdateAsync(housing, cancellationToken);
            }
        }
    }
}

using AutoMapper;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Exceptions;
using RentIt.Housing.Domain.Contracts.Requests.Availabilities;
using FluentValidation;

namespace RentIt.Housing.Domain.Services
{
    public class AvailabilityService
    {
        private readonly HousingService _housingService;
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly IValidator<CreateAvailabilitiesRequest> _createAvailabilitiesRequestValidator;
        private readonly IValidator<UpdateAvailabilitiesRequest> _updateAvailabilitiesRequestValidator;
        private readonly IMapper _mapper;

        public AvailabilityService(
            IAvailabilityRepository availabilityRepository,
            IMapper mapper,
            HousingService housingService,
            IValidator<CreateAvailabilitiesRequest> createAvailabilitiesRequestValidator,
            IValidator<UpdateAvailabilitiesRequest> updateAvailabilitiesRequestValidator)
        {
            _availabilityRepository = availabilityRepository;
            _createAvailabilitiesRequestValidator = createAvailabilitiesRequestValidator;
            _mapper = mapper;
            _housingService = housingService;
            _updateAvailabilitiesRequestValidator = updateAvailabilitiesRequestValidator;
        }

        public async Task<IEnumerable<Availability>> GetAvailabilitiesByHousingIdAsync(
            Guid housingId, 
            CancellationToken cancellationToken)
        {
            return await _availabilityRepository.GetAvailabilitiesByHousingIdAsync(housingId, cancellationToken);
        }

        public async Task AddAvailabilitiesAsync(
            Guid housingId,
            CreateAvailabilitiesRequest request,
            CancellationToken cancellationToken)
        {
            var housing = await _housingService.GetByIdAsync(housingId, cancellationToken);

            if(housing == null)
            {
                throw new NotFoundException("Собственности с таким ID не найдено.");
            }

            await _createAvailabilitiesRequestValidator.ValidateAndThrowAsync(request, cancellationToken);

            var availibilities = _mapper.Map<List<Availability>>(request.AvailabilityDtos, opt =>
            {
                opt.Items["housingId"] = housingId;
            });

            foreach( var availability in availibilities)
            {
                await _availabilityRepository.AddAsync(availability, cancellationToken);
            }

            housing.Housing.Availabilities.AddRange(availibilities);

            await _housingService.UpdateHousingAsync(housing.Housing, cancellationToken);
        }

        public async Task UpdateAvailabilitiesAsync(
            Guid housingId,
            UpdateAvailabilitiesRequest request, 
            CancellationToken cancellationToken)
        {
            await _updateAvailabilitiesRequestValidator.ValidateAndThrowAsync(request, cancellationToken);

            var housing = await _housingService.GetByIdAsync(housingId, cancellationToken);
            if (housing == null)
            {
                throw new NotFoundException($"Жильё с ID {housingId} не найдено.");
            }

            var newAvailabilities = await _availabilityRepository.GetAvailabilitiesByHousingIdAsync(housingId, cancellationToken);

            _mapper.Map(request.AvailabilityDtos, newAvailabilities, opts =>
            {
                opts.Items["housingId"] = housingId;
            });

            housing.Housing.Availabilities = newAvailabilities.ToList();

            foreach( var a in housing.Housing.Availabilities )
            {
                await _availabilityRepository.UpdateAsync(a, cancellationToken);
            }

            await _housingService.UpdateHousingAsync(housing.Housing, cancellationToken);
        }

        public async Task DeleteAvailabilityAsync(
            Guid availabilityId, 
            CancellationToken cancellationToken)
        {
            var availability = await _availabilityRepository.GetAvailabilityByIdAsync(availabilityId, cancellationToken);

            if( availability == null )
            {
                throw new NotFoundException("Периода с таким ID не найдено.");
            }

            var housing = await _housingService.GetByIdAsync(availability.HousingId, cancellationToken);

            housing.Housing.Availabilities.RemoveAll(a => a.AvailabilityId == availabilityId);

            await _availabilityRepository.DeleteAsync(availabilityId, cancellationToken);
            await _housingService.UpdateHousingAsync(housing.Housing, cancellationToken);
        }

        public async Task DeleteAllAvailabilities(
            Guid housingId,
            CancellationToken cancellationToken)
        {
            var housing = await _housingService.GetByIdAsync(housingId, cancellationToken);

            if( housing == null )
            {
                throw new NotFoundException("Собственности с таким ID не найдено.");
            }

            housing.Housing.Availabilities.Clear();

            var availabilities = await _availabilityRepository.GetAvailabilitiesByHousingIdAsync(housingId, cancellationToken);

            foreach( var a in availabilities )
            {
                await _availabilityRepository.DeleteAsync(a.AvailabilityId, cancellationToken);
            }
            
            await _housingService.UpdateHousingAsync(housing.Housing, cancellationToken);
        }
    }
}
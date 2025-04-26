using AutoMapper;
using RentIt.Bookings.Application.Interfaces.Services.Grpc;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Specifications.Bookings;
using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Application.Interfaces.Services;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class GetBookingsByHousingIdUseCase : IGetBookingsByHousingIdUseCase
    {
        private readonly IAppLogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHousingIntegrationService _housingIntegrationsService;
        private readonly IMapper _mapper;

        public GetBookingsByHousingIdUseCase(
            IAppLogger logger, 
            IUnitOfWork unitOfWork,
            IHousingIntegrationService housingIntegrationsService,
            IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _housingIntegrationsService = housingIntegrationsService;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<BookingDto>> ExecuteAsync(
            Guid housingId,
            string authenticatedUserId,
            string authenticatedUserRole,
            GetBookingsByPagesRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Получение бронирований для собственности с ID {HousingId}.", housingId);

            var parseUserIdAttempt = Guid.TryParse(authenticatedUserId, out var authenticatedUserGuid);

            if (!parseUserIdAttempt)
            {
                _logger.LogWarning("Некорректный формат ID пользователя.");

                throw new ArgumentException("Некорректный формат ID пользователя.");
            }

            var housing = await _housingIntegrationsService.GetHousingInfoAsync(housingId);

            if (housing.OwnerId != authenticatedUserGuid && authenticatedUserRole != "Admin")
            {
                _logger.LogWarning("Попытка неавторизованного доступа к бронированиям собственности с ID {HousingId}.", housingId);

                throw new UnauthorizedAccessException("Попытка неавторизованного доступа.");
            }

            var specification = new SearchBookingSpecification(
                                        housingId: housingId,
                                        page: request.page,
                                        pageSize: request.pageSize);

            var bookings = await _unitOfWork.Bookings.GetPaginatedFilteredBookingsAsync(specification, cancellationToken);

            _logger.LogInformation("Найдено бронирований: {TotalCount}", bookings.TotalCount);

            var paginatedDtos = _mapper.Map<PaginatedResult<BookingDto>>(bookings);

            _logger.LogInformation("Преобразование бронирований в DTO завершено");

            return paginatedDtos;
        }
    }
}
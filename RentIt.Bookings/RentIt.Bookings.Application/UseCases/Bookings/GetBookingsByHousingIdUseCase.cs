using AutoMapper;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Services.Grpc;
using RentIt.Bookings.Application.Specifications.Bookings;
using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class GetBookingsByHousingIdUseCase : IGetBookingsByHousingIdUseCase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly HousingIntegrationService _housingIntegrationsService;
        private readonly IMapper _mapper;

        public GetBookingsByHousingIdUseCase(
            ILogger logger, 
            IUnitOfWork unitOfWork,
            HousingIntegrationService housingIntegrationsService,
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
            _logger.Information("Получение бронирований для собственности с ID {HousingId}.", housingId);

            var parseUserIdAttempt = Guid.TryParse(authenticatedUserId, out var authenticatedUserGuid);

            if (!parseUserIdAttempt)
            {
                _logger.Warning("Некорректный формат ID пользователя.");

                throw new ArgumentException("Некорректный формат ID пользователя.");
            }

            var housing = await _housingIntegrationsService.GetHousingInfoAsync(housingId);

            if (housing.OwnerId != authenticatedUserGuid && authenticatedUserRole != "Admin")
            {
                _logger.Warning("Попытка неавторизованного доступа к бронированиям собственности с ID {HousingId}.", housingId);

                throw new UnauthorizedAccessException("Попытка неавторизованного доступа.");
            }

            var specification = new SearchBookingSpecification(
                                        housingId: housingId,
                                        page: request.page,
                                        pageSize: request.pageSize);

            var bookings = await _unitOfWork.Bookings.GetPaginatedFilteredBookingsAsync(specification, cancellationToken);

            _logger.Information("Найдено бронирований: {TotalCount}", bookings.TotalCount);

            var paginatedDtos = _mapper.Map<PaginatedResult<BookingDto>>(bookings);

            _logger.Information("Преобразование бронирований в DTO завершено");

            return paginatedDtos;
        }
    }
}
using AutoMapper;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Specifications.Bookings;
using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Application.Interfaces.Services;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class GetBookingsByUserIdUseCase : IGetBookingsByUserIdUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAppLogger _logger;

        public GetBookingsByUserIdUseCase(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            IAppLogger logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaginatedResult<BookingDto>> ExecuteAsync(
            Guid userId, 
            string authenticatedUserId,
            string authenticatedUserRole,
            GetBookingsByPagesRequest request, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Получение бронирований для пользователя с UserId: {UserId}.", userId);

            if (authenticatedUserId != userId.ToString() && authenticatedUserRole != "Admin")
            {
                _logger.LogWarning("Произошла попытка неавторизованного доступа к данным о бронировании пользователя {UserId}.", userId);

                throw new UnauthorizedAccessException("Попытка неавторизованного доступа.");
            }

            var specification = new SearchBookingSpecification(
                                        userId: userId, 
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

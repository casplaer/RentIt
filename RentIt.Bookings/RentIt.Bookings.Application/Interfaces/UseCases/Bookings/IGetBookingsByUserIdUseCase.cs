using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Interfaces.Repositories;

namespace RentIt.Bookings.Application.Interfaces.UseCases.Bookings
{
    public interface IGetBookingsByUserIdUseCase
    {
        Task<PaginatedResult<BookingDto>> ExecuteAsync(
                                              Guid userId, 
                                              string authenticatedUserId, 
                                              string authenticatedUserRole, 
                                              GetBookingsByPagesRequest request, 
                                              CancellationToken cancellationToken);
    }
}

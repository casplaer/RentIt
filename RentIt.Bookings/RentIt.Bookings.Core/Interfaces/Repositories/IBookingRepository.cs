using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Specitfications;

namespace RentIt.Bookings.Core.Interfaces.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<PaginatedResult<Booking>> GetPaginatedFilteredBookingsAsync(ISpecification<Booking> specification, CancellationToken cancellationToken);
        Task<IEnumerable<Booking>> GetAllFilteredBookingsAsync(ISpecification<Booking> specification, CancellationToken cancellationToken);
        Task<Booking?> GetNextBookingByEndDate(Guid housingId, DateTime endDate, CancellationToken cancellationToken);
        Task<bool> AnyOverlappingBookingAsync(Guid housingId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
        Task<IEnumerable<Booking>> GetBookingsByStatusesAsync(Guid housingId, IEnumerable<BookingStatus> statuses, CancellationToken cancellationToken);
    }
}

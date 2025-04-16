using Microsoft.EntityFrameworkCore;
using RentIt.Bookings.Application.Specifications;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Core.Interfaces.Specitfications;
using RentIt.Bookings.Infrastructure.Data;

namespace RentIt.Bookings.Infrastructure.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(RentItDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Booking>> GetAllFilteredBookingsAsync(
            ISpecification<Booking> specification, 
            CancellationToken cancellationToken)
        {
            var query = _context.Bookings
                .AsQueryable();

            query = ApplySpecification(specification);

            return await query.ToListAsync(cancellationToken);
        }

        public Task<Booking?> GetNextBookingByEndDate(
            Guid housingId, 
            DateTime endDate, 
            CancellationToken cancellationToken)
        {
            var nextBooking = _context.Bookings
                .Where(b => b.HousingId == housingId && b.StartDate > endDate)
                .OrderBy(b => b.StartDate)
                .FirstOrDefaultAsync(cancellationToken);

            return nextBooking;
        }

        public async Task<PaginatedResult<Booking>> GetPaginatedFilteredBookingsAsync(
            ISpecification<Booking> specification, 
            CancellationToken cancellationToken)
        {
            var query = _context.Bookings
                .AsQueryable();

            query = ApplySpecification(specification);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query.ToListAsync(cancellationToken);

            return new PaginatedResult<Booking>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = (int)specification.PageSize,
                CurrentPage = (int)specification.Page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)specification.PageSize)
            };
        }

        public async Task<bool> AnyOverlappingBookingAsync(
            Guid housingId, 
            DateTime startDate, 
            DateTime endDate, 
            CancellationToken cancellationToken)
        {
            return await _context.Bookings
                .AnyAsync(b => b.HousingId == housingId &&
                              (b.Status == BookingStatus.Confirmed ||
                               b.Status == BookingStatus.Paid ||
                               b.Status == BookingStatus.Active) &&
                               b.StartDate < endDate &&
                               b.EndDate > startDate,
                            cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByStatusesAsync(
            Guid housingId, 
            IEnumerable<BookingStatus> statuses, 
            CancellationToken cancellationToken)
        {
            return await _context.Bookings
                .Where(b => b.HousingId == housingId && statuses.Contains(b.Status))
                .ToListAsync(cancellationToken);
        }

        private IQueryable<Booking> ApplySpecification(
                    ISpecification<Booking> specification)
        {
            return SpecificationEvaluator.GetQuery(
                _context.Set<Booking>(),
                specification);
        }
    }
}

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

        public async Task<(DateTime? ChaingStart, DateTime? ChainEnd)> GetCurrentBookingChainAsync(Booking excludedBooking, CancellationToken cancellationToken)
        {
            var validStatuses = new[]
            {
                BookingStatus.Confirmed, 
                BookingStatus.Paid, 
                BookingStatus.Active 
            };

            var bookings = await _context.Bookings
                .Where(b => b.HousingId == excludedBooking.HousingId
                            && b.BookingId != excludedBooking.BookingId
                            && validStatuses.Contains(b.Status))
                .OrderBy(b => b.StartDate)
                .ToListAsync(cancellationToken);

            if (bookings.Count == 0)
            {
                return (null, null);
            }

            var chainStart = bookings.First().StartDate;
            var chainEnd = bookings.First().EndDate;

            for (int i = 1; i < bookings.Count; i++)
            {
                var previous = bookings[i - 1];
                var current = bookings[i];

                var gap = (current.StartDate - previous.EndDate).TotalHours;

                if (gap < 36)
                {
                    chainEnd = current.EndDate;
                }
                else
                {
                    break;
                }
            }

            return (chainStart, chainEnd);
        }

        public async Task<(DateTime? ChaingStart, DateTime? ChainEnd)> GetCurrentBookingChainAsync(Guid housingId, CancellationToken cancellationToken)
        {
            var validStatuses = new[] 
            { 
                BookingStatus.Confirmed, 
                BookingStatus.Paid, 
                BookingStatus.Active 
            };

            var bookings = await _context.Bookings
                .Where(b => b.HousingId == housingId
                            && validStatuses.Contains(b.Status))
                .OrderBy(b => b.StartDate)
                .ToListAsync(cancellationToken);

            if (bookings.Count == 0)
            {
                return (null, null);
            }

            var chainStart = bookings.First().StartDate;
            var chainEnd = bookings.First().EndDate;

            for (int i = 1; i < bookings.Count; i++)
            {
                var previous = bookings[i - 1];
                var current = bookings[i];

                var gap = (current.StartDate - previous.EndDate).TotalHours;

                if (gap < 36)
                {
                    chainEnd = current.EndDate;
                }
                else
                {
                    break;
                }
            }

            return (chainStart, chainEnd);
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

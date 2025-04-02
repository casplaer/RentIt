using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Infrastructure.Data;

namespace RentIt.Bookings.Infrastructure.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(RentItDbContext context)
            : base(context)
        {
        }
    }
}

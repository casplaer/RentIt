using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Infrastructure.Data;

namespace RentIt.Bookings.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RentItDbContext _context;
        private IBookingRepository? _bookingRepository;
        private IPaymentRepository? _paymentRepository;

        public UnitOfWork(
            RentItDbContext context, 
            IBookingRepository? bookingRepository, 
            IPaymentRepository? paymentRepository)
        {
            _context = context;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
        }

        public IBookingRepository Bookings => _bookingRepository;
        public IPaymentRepository Payments => _paymentRepository;

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

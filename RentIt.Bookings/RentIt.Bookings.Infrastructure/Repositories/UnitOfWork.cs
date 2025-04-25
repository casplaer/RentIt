using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Infrastructure.Data;

namespace RentIt.Bookings.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RentItDbContext _context;
        private IBookingRepository? _bookingRepository;
        private IPaymentRepository? _paymentRepository;
        private IMessageRepository? _messageRepository;

        public UnitOfWork(
            RentItDbContext context, 
            IBookingRepository? bookingRepository, 
            IPaymentRepository? paymentRepository,
            IMessageRepository? messageRepository)
        {
            _context = context;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
            _messageRepository = messageRepository;
        }

        public IBookingRepository Bookings => _bookingRepository;
        public IPaymentRepository Payments => _paymentRepository;
        public IMessageRepository Messages => _messageRepository;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Infrastructure.Data;

namespace RentIt.Bookings.Infrastructure.Repositories
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(RentItDbContext context) 
            : base(context)
        {   
        }
    }
}

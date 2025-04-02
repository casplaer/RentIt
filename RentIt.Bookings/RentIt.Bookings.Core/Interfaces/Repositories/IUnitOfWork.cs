namespace RentIt.Bookings.Core.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IBookingRepository Bookings { get; }
        IPaymentRepository Payments { get; }
        Task<int> SaveChangesAsync();
    }
}
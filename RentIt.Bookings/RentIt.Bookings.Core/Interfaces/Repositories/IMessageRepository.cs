using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Core.Interfaces.Repositories
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<IEnumerable<Message>> GetMessagesAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken);
    }
}
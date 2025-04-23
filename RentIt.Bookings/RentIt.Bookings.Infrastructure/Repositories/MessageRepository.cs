using MassTransit;
using Microsoft.EntityFrameworkCore;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Infrastructure.Data;

namespace RentIt.Bookings.Infrastructure.Repositories
{
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(RentItDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Message>> GetMessagesAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken)
        {
            return await _context.Messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                            (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync(cancellationToken);
        }
    }
}
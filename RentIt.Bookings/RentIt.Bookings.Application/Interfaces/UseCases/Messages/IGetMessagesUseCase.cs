using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Application.Interfaces.UseCases.Messages
{
    public interface IGetMessagesUseCase
    {
        Task<IEnumerable<Message>> ExecuteAsync(string userId, Guid otherUserId, CancellationToken cancellationToken);
    }
}
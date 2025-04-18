using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;

namespace RentIt.Bookings.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatHub(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SendMessage(Guid senderId, Guid receiverId, string message, CancellationToken cancellationToken)
        {
            var msg = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.Messages.AddAsync(msg, cancellationToken);

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, message);
        }
    }
}
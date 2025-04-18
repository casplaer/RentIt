using RentIt.Bookings.Application.Interfaces.UseCases.Messages;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Messages
{
    public class GetMessagesUseCase : IGetMessagesUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public GetMessagesUseCase(
            IUnitOfWork unitOfWork,
            ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Message>> ExecuteAsync(string userId, Guid otherUserId, CancellationToken cancellationToken)
        {
            _logger.Information("Обработка запроса на получение сообщений от {UserId} для {OtherUserId}", userId, otherUserId);

            var userIdParseAttempt = Guid.TryParse(userId, out var userGuid);

            if (!userIdParseAttempt)
            {
                _logger.Warning("Некорректный формат ID пользователя, ожидался GUID.");

                throw new ArgumentException("Некорректный формат ID пользователя.");
            }

            var msgs = await _unitOfWork.Messages.GetMessagesAsync(userGuid, otherUserId, cancellationToken);

            return msgs;
        }
    }
}

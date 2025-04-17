using MediatR;
using Microsoft.Extensions.Logging;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Status
{
    public class StatusUpdateCommandHandler : IRequestHandler<StatusUpdateCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<StatusUpdateCommandHandler> _logger;

        public StatusUpdateCommandHandler(IUserRepository userRepository, ILogger<StatusUpdateCommandHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(
            StatusUpdateCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Запрос на смену статуса для пользователя с Id: {UserId}", request.UserId);

            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Пользователь с Id {UserId} не найден", request.UserId);

                throw new NotFoundException("Пользователь не найден.");
            }

            user.Status = user.Status == UserStatus.Inactive ? UserStatus.Active : UserStatus.Inactive;

            _logger.LogInformation("Статус пользователя с Id: {UserId} изменен на: {Status}", request.UserId, user.Status);

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Статус пользователя с Id: {UserId} успешно обновлен", request.UserId);

            return true;
        }
    }
}

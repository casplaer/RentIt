using MediatR;
using RentIt.Users.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Users.Application.Commands.Users.Delete
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public DeleteUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _logger = Log.ForContext<DeleteUserCommandHandler>();
        }

        public async Task<bool> Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Попытка удалить пользователя с ID: {UserId}", request.UserId);

            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.Warning("Пользователь с ID: {UserId} не найден. Удаление невозможно.", request.UserId);
                return false;
            }

            _userRepository.Delete(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.Information("Пользователь с ID: {UserId} успешно удалён.", request.UserId);

            return true;
        }
    }
}

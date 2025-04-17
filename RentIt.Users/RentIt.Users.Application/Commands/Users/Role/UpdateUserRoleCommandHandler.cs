using MediatR;
using Microsoft.Extensions.Logging;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Role
{
    public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<UpdateUserRoleCommandHandler> _logger;

        public UpdateUserRoleCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository, ILogger<UpdateUserRoleCommandHandler> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(
            UpdateUserRoleCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Запрос на обновление роли для пользователя с Id: {UserId}", request.UserId);

            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Пользователь с Id {UserId} не найден", request.UserId);

                throw new NotFoundException("Пользователь не найден.");
            }

            var newRole = await _roleRepository.GetRoleByNameAsync(request.NewRole, cancellationToken);
            if (newRole == null)
            {
                _logger.LogWarning("Роль {Role} не найдена", request.NewRole);

                throw new NotFoundException($"Роль {request.NewRole} не найдена.");
            }

            _logger.LogInformation("Обновление роли пользователя с Id: {UserId} на роль {NewRole}", request.UserId, newRole.Name);

            user.RoleId = newRole.RoleId;
            user.Role = newRole;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Роль пользователя с Id: {UserId} успешно обновлена на {NewRole}", request.UserId, newRole.Name);

            return true;
        }
    }
}

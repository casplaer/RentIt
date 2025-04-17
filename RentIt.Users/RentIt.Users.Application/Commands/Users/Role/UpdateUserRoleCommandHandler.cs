using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Users.Application.Commands.Users.Role
{
    public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger _logger;

        public UpdateUserRoleCommandHandler(
            IUserRepository userRepository, 
            IRoleRepository roleRepository, 
            ILogger logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(
            UpdateUserRoleCommand request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Запрос на обновление роли для пользователя с Id: {UserId}", request.UserId);

            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.Warning("Пользователь с Id {UserId} не найден", request.UserId);

                throw new NotFoundException("Пользователь не найден.");
            }

            var newRole = await _roleRepository.GetRoleByNameAsync(request.NewRole, cancellationToken);
            if (newRole == null)
            {
                _logger.Warning("Роль {Role} не найдена", request.NewRole);

                throw new NotFoundException($"Роль {request.NewRole} не найдена.");
            }

            _logger.Information("Обновление роли пользователя с Id: {UserId} на роль {NewRole}", request.UserId, newRole.RoleName);

            user.RoleId = newRole.RoleId;
            user.Role = newRole;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.Information("Роль пользователя с Id: {UserId} успешно обновлена на {NewRole}", request.UserId, newRole.RoleName);

            return true;
        }
    }
}

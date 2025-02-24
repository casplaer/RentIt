using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Role
{
    public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UpdateUserRoleCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<bool> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Пользователь не найден.");
            }

            var newRole = await _roleRepository.GetRoleByNameAsync(request.NewRole, cancellationToken);
            if (newRole == null)
            {
                throw new NotFoundException($"Роль {request.NewRole} не найдена.");
            }

            user.RoleId = newRole.RoleId;
            user.Role = newRole;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }

}

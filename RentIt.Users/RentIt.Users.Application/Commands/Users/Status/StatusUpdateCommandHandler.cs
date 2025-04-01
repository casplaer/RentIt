using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Status
{
    public class StatusUpdateCommandHandler : IRequestHandler<StatusUpdateCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        public StatusUpdateCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;    
        }

        public async Task<bool> Handle(
            StatusUpdateCommand request, 
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Пользователь не найден.");
            }    

            user.Status = user.Status == UserStatus.Inactive ? UserStatus.Active : UserStatus.Inactive;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

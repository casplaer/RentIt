using MediatR;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Update
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailNormalizer _emailNormalizer;

        public UpdateUserCommandHandler(
            IUserRepository userRepository,
            IEmailNormalizer emailNormalizer)
        {
            _userRepository = userRepository;
            _emailNormalizer = emailNormalizer;
        }

        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
                return false;

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.NormalizedEmail = _emailNormalizer.NormalizeEmail(request.Email);
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

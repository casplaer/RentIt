using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Account
{
    public class ConfirmAccountCommandHandler : IRequestHandler<ConfirmAccountCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountTokenRepository _accountTokenRepository;

        public ConfirmAccountCommandHandler(
            IUserRepository userRepository,
            IAccountTokenRepository accountTokenRepository)
        {
            _userRepository = userRepository;
            _accountTokenRepository = accountTokenRepository;
        }

        public async Task<bool> Handle(
            ConfirmAccountCommand request, 
            CancellationToken cancellationToken)
        {
            var tokenEntity = await _accountTokenRepository.GetTokenAsync(
                request.UserId,
                request.Token,
                TokenType.Confirmation,
                cancellationToken
            );

            if (tokenEntity == null || tokenEntity.Expiration < DateTime.UtcNow)
            {
                throw new NotFoundException("Неверная или просроченная ссылка для восстановления пароля.");
            }

            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Пользователь не найден.");
            }

            user.Status = UserStatus.Active;

            _accountTokenRepository.Delete(tokenEntity);

            await _userRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

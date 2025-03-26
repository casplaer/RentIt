using MediatR;
using Microsoft.EntityFrameworkCore;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;
using System.Data.Entity;

namespace RentIt.Users.Application.Commands.Users.Account
{
    public class ConfirmAccountCommandHandler : IRequestHandler<ConfirmAccountCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRepository<AccountToken> _accountTokenRepository;

        public ConfirmAccountCommandHandler(
            IUserRepository userRepository,
            IRepository<AccountToken> accountTokenRepository)
        {
            _userRepository = userRepository;
            _accountTokenRepository = accountTokenRepository;
        }

        public async Task<bool> Handle(
            ConfirmAccountCommand request, 
            CancellationToken cancellationToken)
        {
            var tokenEntity = (await _accountTokenRepository.GetAllAsync(cancellationToken))
                .Where(t => t.UserId == request.UserId &&
                            t.Token == request.Token &&
                            t.TokenType == TokenType.Confirmation &&
                            t.Expiration > DateTime.UtcNow)
                .FirstOrDefault();

            if (tokenEntity == null || tokenEntity.Expiration < DateTime.UtcNow)
            {
                throw new NotFoundException("Ссылка устарела.");
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

using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.RefreshToken
{
    public class ValidateRefreshTokenCommandHandler : IRequestHandler<ValidateRefreshTokenCommand, User>
    {
        private readonly IUserRepository _userRepository;
        public ValidateRefreshTokenCommandHandler(
            IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> Handle(ValidateRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);

            if(user == null)
            {
                throw new NotFoundException("Сессия устарела. Требуется повторный вход.");
            }

            return user;
        }
    }
}

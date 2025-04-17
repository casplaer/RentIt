using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Queries.Users;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Users.Application.Queries
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, User>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public GetUserByIdQueryHandler(
            IUserRepository userRepository,
            ILogger logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.Information("Запрос на получение пользователя по ID {UserId}.", request.UserId);

            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            
            if(user == null)
            {
                _logger.Warning("Пользователь с ID {UserId} не найден.", request.UserId);

                throw new NotFoundException("Пользователь не найден.");
            }
            
            return user;
        }
    }
}

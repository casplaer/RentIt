using MediatR;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Users.Application.Queries.Users
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ICollection<User>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public GetAllUsersQueryHandler(
            IUserRepository userRepository,
            ILogger logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ICollection<User>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            _logger.Information("Запрос на получение всех пользователей.");

            return await _userRepository.GetAllAsync(cancellationToken);
        }
    }
}

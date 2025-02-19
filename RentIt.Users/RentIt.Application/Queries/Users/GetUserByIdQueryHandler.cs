using MediatR;
using RentIt.Users.Application.Queries.Users;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Queries
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, User>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        }
    }
}

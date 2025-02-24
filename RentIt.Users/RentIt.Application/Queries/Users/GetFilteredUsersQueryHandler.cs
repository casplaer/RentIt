using AutoMapper;
using MediatR;
using RentIt.Users.Contracts.DTO.Users;
using RentIt.Users.Contracts.Responses.Users;
using RentIt.Users.Core.Interfaces.Repositories;
using LinqKit;
using RentIt.Users.Core.Entities;

namespace RentIt.Users.Application.Queries.Users
{
    public class GetFilteredUsersQueryHandler : IRequestHandler<GetFilteredUsersQuery, GetEventsResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetFilteredUsersQueryHandler(
            IUserRepository userRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<GetEventsResponse> Handle(GetFilteredUsersQuery request, CancellationToken cancellationToken)
        {
            if (request == null)
            { 
                throw new ArgumentNullException(nameof(request), "Отсутствуют данные для фильтрации.");
            }

            var predicate = PredicateBuilder.New<User>(true);

            if (!string.IsNullOrEmpty(request.FirstName))
                predicate = predicate.And(u => u.FirstName.Contains(request.FirstName));
            if (!string.IsNullOrEmpty(request.LastName))
                predicate = predicate.And(u => u.LastName.Contains(request.LastName));
            if (!string.IsNullOrEmpty(request.Email))
                predicate = predicate.And(u => u.Email.Contains(request.Email));
            if (!string.IsNullOrEmpty(request.Country))
                predicate = predicate.And(u => u.Profile != null && u.Profile.Country.Contains(request.Country));
            if (!string.IsNullOrEmpty(request.City))
                predicate = predicate.And(u => u.Profile != null && u.Profile.City.Contains(request.City));
            if (!string.IsNullOrEmpty(request.PhoneNumber))
                predicate = predicate.And(u => u.Profile != null && u.Profile.PhoneNumber.Contains(request.PhoneNumber));

            var users = await _userRepository
                .GetFilteredUsersAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Country,
                request.City,
                request.PhoneNumber,
                request.Page,
                request.PageSize,
                cancellationToken);

            return new GetEventsResponse(
                    Users: _mapper.Map<ICollection<UserDTO>>(users.Items),
                    PageNumber: users.CurrentPage,
                    TotalPages: users.TotalPages
                );
        }
    }
}

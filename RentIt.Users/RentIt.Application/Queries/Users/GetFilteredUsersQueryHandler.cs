using AutoMapper;
using MediatR;
using RentIt.Users.Contracts.DTO.Users;
using RentIt.Users.Contracts.Responses.Users;
using RentIt.Users.Core.Interfaces.Repositories;
using LinqKit;
using RentIt.Users.Core.Entities;
using RentIt.Users.Application.Specifications.Users;

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

            var specification = new GetFilteredUsersSpecification(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Country,
                request.City,
                request.PhoneNumber,
                request.Page,
                request.PageSize
            );

            var users = await _userRepository
                .GetFilteredUsersAsync(
                specification,
                cancellationToken);

            return new GetEventsResponse(
                    Users: _mapper.Map<ICollection<UserDTO>>(users.Items),
                    PageNumber: users.CurrentPage,
                    TotalPages: users.TotalPages
                );
        }
    }
}

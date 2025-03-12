using AutoMapper;
using MediatR;
using RentIt.Users.Contracts.Dto.Users;
using RentIt.Users.Contracts.Responses.Users;
using RentIt.Users.Core.Interfaces.Repositories;
using RentIt.Users.Application.Specifications.Users;

namespace RentIt.Users.Application.Queries.Users
{
    public class GetFilteredUsersQueryHandler : IRequestHandler<GetFilteredUsersQuery, GetUsersResponse>
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

        public async Task<GetUsersResponse> Handle(GetFilteredUsersQuery request, CancellationToken cancellationToken)
        {
            var specification = new GetFilteredUsersSpecification(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Role,
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

            return new GetUsersResponse(
                    Users: _mapper.Map<ICollection<UserDto>>(users.Items),
                    PageNumber: users.CurrentPage,
                    TotalPages: users.TotalPages
                );
        }
    }
}

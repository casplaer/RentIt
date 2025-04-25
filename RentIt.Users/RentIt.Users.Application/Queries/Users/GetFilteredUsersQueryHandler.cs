using AutoMapper;
using MediatR;
using RentIt.Users.Contracts.Dto.Users;
using RentIt.Users.Contracts.Responses.Users;
using RentIt.Users.Core.Interfaces.Repositories;
using RentIt.Users.Application.Specifications.Users;
using Serilog;

namespace RentIt.Users.Application.Queries.Users
{
    public class GetFilteredUsersQueryHandler : IRequestHandler<GetFilteredUsersQuery, GetUsersResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public GetFilteredUsersQueryHandler(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GetUsersResponse> Handle(GetFilteredUsersQuery request, CancellationToken cancellationToken)
        {
            _logger.Information("Запрос на получение пользователей по фильтрам.");

            var specification = new GetFilteredUsersSpecification(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Role,
                request.Status,
                request.Country,
                request.City,
                request.PhoneNumber,
                request.Page,
                request.PageSize
            );

            _logger.Information("Обращение к базе данных");

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

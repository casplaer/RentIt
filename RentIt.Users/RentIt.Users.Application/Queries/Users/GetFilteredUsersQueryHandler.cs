using AutoMapper;
using MediatR;
using RentIt.Users.Contracts.DTO.Users;
using RentIt.Users.Contracts.Responses.Users;
using RentIt.Users.Core.Interfaces.Repositories;
using RentIt.Users.Application.Specifications.Users;
using FluentValidation;

namespace RentIt.Users.Application.Queries.Users
{
    public class GetFilteredUsersQueryHandler : IRequestHandler<GetFilteredUsersQuery, GetUsersResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<GetFilteredUsersQuery> _validator;

        public GetFilteredUsersQueryHandler(
            IUserRepository userRepository,
            IMapper mapper,
            IValidator<GetFilteredUsersQuery> validator)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<GetUsersResponse> Handle(GetFilteredUsersQuery request, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(request);

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
                    Users: _mapper.Map<ICollection<UserDTO>>(users.Items),
                    PageNumber: users.CurrentPage,
                    TotalPages: users.TotalPages
                );
        }
    }
}

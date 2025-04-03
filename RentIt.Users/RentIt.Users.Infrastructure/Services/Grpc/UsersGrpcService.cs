using Grpc.Core;
using MediatR;
using RentIt.Protos.Users;
using RentIt.Users.Application.Queries.Users;

namespace RentIt.Users.Infrastructure.Services.Grpc
{
    public class UsersGrpcService : UsersService.UsersServiceBase
    {
        private readonly IMediator _mediator;

        public UsersGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.UserId, out var userGuid))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Неверный формат user_id. Ожидается GUID."));
            }

            var userDto = await _mediator.Send(new GetUserByIdQuery(userGuid));

            if (userDto == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Пользователь с ID {userGuid} не найден."));
            }

            return new GetUserResponse
            {
                UserId = userDto.UserId.ToString(),
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                PhoneNumber = userDto.Profile.PhoneNumber
            };
        }
    }
}

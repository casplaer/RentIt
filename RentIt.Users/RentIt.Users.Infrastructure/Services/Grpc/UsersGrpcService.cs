using Grpc.Core;
using MediatR;
using RentIt.Protos.Users;
using RentIt.Users.Application.Queries.Users;
using Serilog;

namespace RentIt.Users.Infrastructure.Services.Grpc
{
    public class UsersGrpcService : UsersService.UsersServiceBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public UsersGrpcService(
            IMediator mediator,
            ILogger logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
        {
            _logger.Information("Получение информации о пользователе с ID {UserId} в gRPC сервисе.", request.UserId);

            if (!Guid.TryParse(request.UserId, out var userGuid))
            {
                _logger.Warning("Неверный формат user_id. Ожидается GUID.");

                throw new RpcException(new Status(StatusCode.InvalidArgument, "Неверный формат user_id. Ожидается GUID."));
            }

            var userDto = await _mediator.Send(new GetUserByIdQuery(userGuid));

            if (userDto == null)
            {
                _logger.Warning("Пользователь с ID {UserId} не найден.", userGuid);

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

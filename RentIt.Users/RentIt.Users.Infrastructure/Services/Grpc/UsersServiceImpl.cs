using Grpc.Core;
using RentIt.Protos.Users;

namespace RentIt.Users.Infrastructure.Services.Grpc
{
    public class UsersServiceImpl : UsersService.UsersServiceBase
    {
        public override Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
        {
            return base.GetUser(request, context);
        }
    }
}

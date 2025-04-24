using RentIt.Bookings.Contracts.Dto;

namespace RentIt.Bookings.Application.Interfaces.Services.Grpc
{
    public interface IUserIntegrationService
    {
        Task<UserInfoDto> GetUserInfoAsync(Guid userId);
    }
}
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Specifications;

namespace RentIt.Users.Core.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
        Task<ICollection<User>> GetUsersByStatusAsync(UserStatus status, CancellationToken cancellationToken);
        Task<PaginatedResult<User>> GetFilteredUsersAsync(ISpecification<User> specification, CancellationToken cancellationToken);
    }
}

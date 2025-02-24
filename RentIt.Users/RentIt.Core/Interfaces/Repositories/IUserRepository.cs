using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;

namespace RentIt.Users.Core.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<ICollection<User>> GetUsersByRoleAsync(Guid roleId, CancellationToken cancellationToken);
        Task<User?> GetUserByFirstNameAsync(string firstName, CancellationToken cancellationToken);
        Task<User?> GetUserByLastNameAsync(string lastName, CancellationToken cancellationToken);
        Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
        Task<ICollection<User>> GetUsersByStatusAsync(UserStatus status, CancellationToken cancellationToken);
        Task<PaginatedResult<User>> GetFilteredUsersAsync(
            string? FirstName,
            string? lastName,
            string? email,
            string? country,
            string? city,
            string? phoneNumber, 
            int page, 
            int pageSize, 
            CancellationToken cancellationToken);

        //Методы с использованием навигационного свойства UserProfile
        Task<ICollection<User>> GetUsersByCountry(string country, CancellationToken cancellationToken);
        Task<ICollection<User>> GetUsersByCity(string city, CancellationToken cancellationToken);
        Task<User?> GetUserByPhoneNumber(string phoneNumber, CancellationToken cancellationToken);
    }
}

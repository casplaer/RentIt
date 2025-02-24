using Microsoft.EntityFrameworkCore;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;
using RentIt.Users.Infrastructure.Data;
using RentIt.Users.Infrastructure.Extensions;

namespace RentIt.Users.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(RentItDbContext context) 
            : base(context) { }

        public override async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
        }

        public override async Task<ICollection<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.Role)
                .ToListAsync(cancellationToken);
        }

        public async Task<ICollection<User>> GetUsersByRoleAsync(Guid roleId, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(u => u.RoleId == roleId)
                .ToListAsync(cancellationToken);
        }

        public async Task<User?> GetUserByFirstNameAsync(string firstName, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.FirstName == firstName);
        }
        
        public async Task<User?> GetUserByLastNameAsync(string lastName, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.LastName == lastName);
        }

        public async Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
        }

        public async Task<ICollection<User>> GetUsersByStatusAsync(UserStatus status, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, cancellationToken);
        }

        public async Task<PaginatedResult<User>> GetFilteredUsersAsync(
            string? firstName,
            string? lastName,
            string? email,
            string? country,
            string? city,
            string? phoneNumber,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {

            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Profile)
                .AsQueryable();

            query = query
                .ApplyFilter(u => u.FirstName.Contains(firstName!), !string.IsNullOrEmpty(firstName))
                .ApplyFilter(u => u.LastName.Contains(lastName!), !string.IsNullOrEmpty(lastName))
                .ApplyFilter(u => u.Email.Contains(email!), !string.IsNullOrEmpty(email))
                .ApplyFilter(u => u.Profile.Country.Contains(country!), !string.IsNullOrEmpty(country))
                .ApplyFilter(u => u.Profile.City.Contains(city!), !string.IsNullOrEmpty(city))
                .ApplyFilter(u => u.Profile.PhoneNumber.Contains(phoneNumber!), !string.IsNullOrEmpty(phoneNumber));

            var totalCount = await query.CountAsync(cancellationToken);
            var skip = (Math.Max(1, page) - 1) * pageSize;

            var items = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<User>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        //Методы с использованием навигационного свойства UserProfile
        public async Task<ICollection<User>> GetUsersByCountry(string country, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(u => u.Profile.Country == country)
                .ToListAsync(cancellationToken);
        }

        public async Task<ICollection<User>> GetUsersByCity(string city, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(u => u.Profile.City == city)
                .ToListAsync(cancellationToken);
        }

        public async Task<User?> GetUserByPhoneNumber(string phoneNumber, CancellationToken cancellationToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Profile.PhoneNumber == phoneNumber); 
        }
    }
}

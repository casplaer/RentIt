using Microsoft.EntityFrameworkCore;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;
using RentIt.Users.Infrastructure.Data;

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

        public async Task<ICollection<User>> GetUsersByRoleAsync(Guid roleId, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(u => u.RoleId == roleId)
                .ToListAsync(cancellationToken);
        }

        public async Task<User?> GetUserByFirstNameAsync(string firstName, CancellationToken cancellationToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.FirstName == firstName);
        }
        
        public async Task<User?> GetUserByLastNameAsync(string lastName, CancellationToken cancellationToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.LastName == lastName);
        }

        public async Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
        }

        public async Task<ICollection<User>> GetUsersByStatusAsync(UserStatus status, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(u => u.Status == status)
                .ToListAsync(cancellationToken);
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

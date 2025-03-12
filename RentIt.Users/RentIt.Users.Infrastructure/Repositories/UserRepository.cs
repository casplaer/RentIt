using Microsoft.EntityFrameworkCore;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;
using RentIt.Users.Application.Specifications;
using RentIt.Users.Infrastructure.Data;
using RentIt.Users.Core.Interfaces.Specifications;

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
                .FirstOrDefaultAsync(u=>u.UserId == id, cancellationToken);
        }

        public override async Task<ICollection<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .ToListAsync(cancellationToken);
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
            ISpecification<User> specification,
            CancellationToken cancellationToken)
        {

            var query = _context.Users
                .AsQueryable();

            query = ApplySpecification(specification);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query.ToListAsync(cancellationToken);

            return new PaginatedResult<User>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = specification.PageSize,
                CurrentPage =specification.Page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)specification.PageSize)
            };
        }

        private IQueryable<User> ApplySpecification(
            ISpecification<User> specification)
        {
            return SpecificationEvaluator.GetQuery(
                _context.Set<User>(),
                specification);
        }
    }
}

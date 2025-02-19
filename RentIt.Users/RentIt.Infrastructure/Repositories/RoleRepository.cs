using Microsoft.EntityFrameworkCore;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Interfaces.Repositories;
using RentIt.Users.Infrastructure.Data;

namespace RentIt.Users.Infrastructure.Repositories
{
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(RentItDbContext context)
            : base(context) { }

        public async Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleName);
        }

        public async Task<int> CountUsersInRoleAsync(Guid roleid, CancellationToken cancellationToken)
        {
            return await _context.Roles
                .Where(r => r.RoleId == roleid)
                .Select(r => r.Users.Count)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}

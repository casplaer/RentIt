using RentIt.Users.Core.Entities;

namespace RentIt.Users.Core.Interfaces.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken);
    }
}

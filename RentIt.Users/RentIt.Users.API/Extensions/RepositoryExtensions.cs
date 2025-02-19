using RentIt.Users.Core.Interfaces.Repositories;
using RentIt.Users.Infrastructure.Repositories;

namespace RentIt.Users.API.Extensions
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();

            return services;
        }
    }
}

using RentIt.Users.Application.Queries.Users;

namespace RentIt.Users.API.Extensions
{
    public static class ServiceConfigurationExtensions
    {
        public static void AddCoreServices(this IServiceCollection services)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(typeof(GetAllUsersQuery).Assembly);
            });
        }
    }
}

using RentIt.Users.API.Middleware;
using RentIt.Users.Application.Mappings;

namespace RentIt.Users.API.Extensions
{
    public static class MappingExtensions
    {
        public static IServiceCollection MapAllProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(UserDTOProfile));
            services.AddAutoMapper(typeof(UserUpdateProfile));

            return services;
        }
    }
}

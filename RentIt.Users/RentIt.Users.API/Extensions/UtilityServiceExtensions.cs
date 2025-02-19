using RentIt.Users.Application.Interfaces;
using RentIt.Users.Infrastructure.Data;
using RentIt.Users.Infrastructure.Services;

namespace RentIt.Users.API.Extensions
{
    public static class UtilityServiceExtensions
    {
        public static IServiceCollection AddApplicationUtilities(this IServiceCollection services)
        {
            services.AddScoped<IEmailNormalizer, EmailNormalizer>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<DatabaseSeeder>();

            return services;
        }
    }
}

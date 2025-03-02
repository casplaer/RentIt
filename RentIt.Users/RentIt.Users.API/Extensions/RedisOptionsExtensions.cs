using StackExchange.Redis;

namespace RentIt.Users.API.Extensions
{
    public static class RedisOptionsExtensions
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionString = configuration.GetConnectionString("RedisConnection");
                return ConnectionMultiplexer.Connect(connectionString);
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisConnection");
                options.InstanceName = "RentIt";
            });

            return services;
        }
    }
}

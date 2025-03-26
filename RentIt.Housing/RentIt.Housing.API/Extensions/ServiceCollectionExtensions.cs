using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using RentIt.Protos.Users;
using Serilog;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace RentIt.Housing.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedis(
            this IServiceCollection services, 
            IConfiguration configuration)
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

        public static IServiceCollection AddHousingHangfire(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetConnectionString("RedisConnection");

            services.AddHangfire(config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseRedisStorage(redisConnectionString, new RedisStorageOptions
                        {
                            Db = 2,
                            Prefix = "hangfire:"
                        });
            });

            services.AddHangfireServer();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                    ClockSkew = TimeSpan.Zero,
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["AccessToken"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    },

                    OnTokenValidated = async context =>
                    {
                        var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
                        var token = context.Request.Cookies["AccessToken"];
                        var handler = new JwtSecurityTokenHandler();
                        if (handler.ReadToken(token) is JwtSecurityToken jwtToken)
                        {
                            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                            if (string.IsNullOrEmpty(jti))
                            {
                                throw new SecurityTokenException("Токен не содержит уникального идентификатора (jti).");
                            }

                            var storedToken = await cache.GetStringAsync($"access:{jti}");
                            if (string.IsNullOrEmpty(storedToken) || storedToken != token)
                            {
                                throw new SecurityTokenInvalidTypeException("Токен некорректен или отозван.");
                            }
                        }
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy =>
                    policy.RequireRole("Admin"));
                options.AddPolicy("LandlordPolicy", policy =>
                    policy.RequireRole("Landlord"));
            });

            return services;
        }

        public static IServiceCollection AddGrpc(this IServiceCollection services)
        {
            services.AddGrpcClient<UsersService.UsersServiceClient>(options =>
            {
                options.Address = new Uri("https://localhost:7108");
            });

            return services;
        }

        public static IServiceCollection AddLogging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            services.AddSingleton<Serilog.ILogger>(Log.Logger);

            return services;
        }
    }
}

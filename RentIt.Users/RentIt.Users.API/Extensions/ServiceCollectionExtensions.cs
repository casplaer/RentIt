﻿using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace RentIt.Users.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedis(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionString = configuration.GetConnectionString("RedisUsersConnection");
                return ConnectionMultiplexer.Connect(connectionString);
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisUsersConnection");
                options.InstanceName = "RentIt";
            });

            return services;
        }

        public static IServiceCollection AddUsersHangfire(
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
                            Db = 1,
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
                {
                    policy.RequireRole("Admin");
                    policy.RequireClaim("status", "Active");
                });
                options.AddPolicy("LandlordPolicy", policy =>
                {
                    policy.RequireRole("Landlord");
                    policy.RequireClaim("status", "Active");
                });
            });

            return services;
        }
    }
}

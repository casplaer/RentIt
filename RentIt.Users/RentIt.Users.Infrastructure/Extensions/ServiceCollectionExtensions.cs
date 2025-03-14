﻿using Microsoft.Extensions.DependencyInjection;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Interfaces.Repositories;
using RentIt.Users.Infrastructure.Repositories;
using RentIt.Users.Infrastructure.Services;

namespace RentIt.Users.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationUtilities(this IServiceCollection services)
        {
            services.AddScoped<IEmailNormalizer, EmailNormalizer>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtProvider, JwtProvider>();

            return services;
        }

        public static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();

            return services;
        }
    }
}

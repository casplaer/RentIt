using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RentIt.Users.Application.Commands.Users.Create;
using RentIt.Users.Application.Commands.Users.Update;
using RentIt.Users.Application.Mappings;
using RentIt.Users.Application.Queries.Users;
using RentIt.Users.Application.Validators;
using RentIt.Users.Contracts.Requests.Users;

namespace RentIt.Users.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddScoped<IValidator<GetUsersRequest>, GetUsersRequestValidator>();
            services.AddScoped<IValidator<GetFilteredUsersQuery>, GetFilteredUsersQueryValidator>();
            services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
            services.AddScoped<IValidator<UpdateUserCommand>, UpdateUserCommandValidator>();

            return services;
        }

        public static IServiceCollection AddMediatR(this IServiceCollection services)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(typeof(GetAllUsersQuery).Assembly);
            });

            return services;
        }

        public static IServiceCollection MapAllProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(UserDtoProfile));
            services.AddAutoMapper(typeof(UserUpdateProfile));

            return services;
        }
    }
}

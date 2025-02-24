using FluentValidation;
using RentIt.Users.Contracts.Requests.Users;
using RentIt.Users.Infrastructure.Validators;

namespace RentIt.Users.API.Extensions
{
    public static class ValidatorsExtension
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<GetUsersRequest>, GetUsersRequestValidator>();

            return services;    
        }
    }
}

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RentIt.Housing.Domain.Contracts.Requests.Availabilities;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Mappings.Availabilities;
using RentIt.Housing.Domain.Mappings.Housing;
using RentIt.Housing.Domain.Services;
using RentIt.Housing.Domain.Validators;

namespace RentIt.Housing.Domain.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMappingProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(HousingProfile));
            services.AddAutoMapper(typeof(UpdateHousingRequestProfile));
            services.AddAutoMapper(typeof(AvailabilityProfile));

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<CreateHousingRequest>, CreateHousingRequestValidator>();
            services.AddScoped<IValidator<GetFilteredHousingsRequest>, GetFilteredHousingsRequestValidator>();
            services.AddScoped<IValidator<UpdateHousingRequest>, UpdateHousingRequestValidator>();
            services.AddScoped<IValidator<CreateAvailabilitiesRequest>, CreateAvailabilitiesRequestValidator>();
            services.AddScoped<IValidator<UpdateAvailabilitiesRequest>, UpdateAvailabilitiesRequestValidator>();

            return services;
        }

        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<HousingService>();
            services.AddScoped<HousingImageService>();
            services.AddScoped<ReviewService>();
            services.AddScoped<AvailabilityService>();

            return services;
        }
    }
}

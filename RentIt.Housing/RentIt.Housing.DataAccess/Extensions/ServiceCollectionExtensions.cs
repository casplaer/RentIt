using Microsoft.Extensions.DependencyInjection;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.DataAccess.Repositories;

namespace RentIt.Housing.DataAccess.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IHousingRepository, HousingRepository>();
            services.AddScoped<IHousingImageRepository, HousingImageRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();

            return services;
        }
    }
}
